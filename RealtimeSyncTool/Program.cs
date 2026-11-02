using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RealtimeSyncTool;

internal class Program
{
    private const string DeviceIp = "192.168.29.158";
    private static readonly int[] CandidatePorts = new[] { 5005, 4370 };
    private static int _activePort = 5005;
    private const string CloudServerUrl = "https://hrattendance.tryasp.net";
    private const string CacheFile = "synced_logs.json";

    private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    private static HashSet<string> _syncedKeys = new HashSet<string>();

    private static async Task Main(string[] args)
    {
        Console.Title = "Realtime Biometric IP Sync Tool - HR Attendance";
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("==================================================================");
        Console.WriteLine("    REALTIME BIOMETRIC IP SYNC TOOL (100% READ-ONLY)              ");
        Console.WriteLine("==================================================================");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[*] Target Biometric IP: {DeviceIp} (Ports: 5005 / 4370)");
        Console.WriteLine($"[*] Cloud Portal URL:    {CloudServerUrl}");
        Console.WriteLine($"[*] Safety Mode:         100% READ-ONLY (No machine data will be modified)");
        Console.ResetColor();
        Console.WriteLine("------------------------------------------------------------------");

        LoadSyncedCache();

        // 1. Health check cloud
        Console.Write("[1/2] Connecting to Cloud Portal API... ");
        bool cloudOk = await CheckCloudStatusAsync();
        if (cloudOk)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("ONLINE (Ready)");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARNING (Portal offline or slow, will retry)");
            Console.ResetColor();
        }

        // 2. Loop sync
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\n[2/2] Starting Biometric Polling Loop (Press Ctrl+C to stop)...");
        Console.ResetColor();
        Console.WriteLine("Listening for employee punches on machine...\n");

        int checkCount = 0;
        while (true)
        {
            checkCount++;
            try
            {
                bool connected = false;
                foreach (var port in CandidatePorts)
                {
                    using var client = new ZkClient(DeviceIp, port);
                    if (client.Connect())
                    {
                        connected = true;
                        _activePort = port;
                        var logs = client.ReadAttendanceLogs();
                        if (logs.Count > 0)
                        {
                            int newPunches = 0;
                            foreach (var log in logs)
                            {
                                string key = $"{log.UserId}_{log.PunchTime:yyyyMMddHHmmss}";
                                if (!_syncedKeys.Contains(key))
                                {
                                    bool synced = await SyncPunchToCloudAsync(log);
                                    if (synced)
                                    {
                                        _syncedKeys.Add(key);
                                        newPunches++;
                                    }
                                }
                            }

                            if (newPunches > 0)
                            {
                                SaveSyncedCache();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"[{DateTime.Now:hh:mm:ss tt}] Synced {newPunches} new punch(es) to Web Portal!");
                                Console.ResetColor();
                            }
                        }
                        break;
                    }
                }

                if (!connected)
                {
                    if (checkCount % 6 == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine($"[{DateTime.Now:hh:mm:ss tt}] Waiting for machine connection at {DeviceIp}:5005 / 4370 (WiFi)...");
                        Console.ResetColor();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] Sync loop exception: {ex.Message}");
                Console.ResetColor();
            }

            await Task.Delay(10000);
        }
    }

    private static async Task<bool> CheckCloudStatusAsync()
    {
        try
        {
            var res = await HttpClient.GetAsync($"{CloudServerUrl}/api/biometric/status");
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> SyncPunchToCloudAsync(BiometricRecord record)
    {
        try
        {
            var payload = new
            {
                userId = record.UserId,
                punchTime = record.PunchTime.ToString("yyyy-MM-dd HH:mm:ss"),
                deviceId = $"Realtime-{DeviceIp}",
                type = record.InOutMode == 0 ? "CheckIn" : "CheckOut"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await HttpClient.PostAsync($"{CloudServerUrl}/api/biometric/punch", content);
            if (response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"  -> [SYNCED] Employee ID: {record.UserId,-6} | Time: {record.PunchTime:hh:mm tt} | Type: {payload.type}");
                Console.ResetColor();
                return true;
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  -> [NOTICE] Employee ID {record.UserId}: {body}");
                Console.ResetColor();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[!] Cloud sync post failed: {ex.Message}");
            return false;
        }
    }

    private static void LoadSyncedCache()
    {
        try
        {
            if (File.Exists(CacheFile))
            {
                var json = File.ReadAllText(CacheFile);
                var list = JsonSerializer.Deserialize<List<string>>(json);
                if (list != null)
                {
                    _syncedKeys = new HashSet<string>(list);
                }
            }
        }
        catch { }
    }

    private static void SaveSyncedCache()
    {
        try
        {
            var recent = _syncedKeys.TakeLast(5000).ToList();
            var json = JsonSerializer.Serialize(recent);
            File.WriteAllText(CacheFile, json);
        }
        catch { }
    }
}
