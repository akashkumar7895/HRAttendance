/**
 * Daily Attendance & Punch Station
 * Real-time Live Clock, 1-Click Punch In, Punch Out & Live Stopwatch
 */

document.addEventListener('DOMContentLoaded', function () {
    const station = document.getElementById('punchWidgetStation');
    if (!station) return;

    // --- DOM Elements ---
    const clockEl = document.getElementById('liveDigitalClock');
    const dateEl = document.getElementById('liveCurrentDate');
    const timerDisplay = document.getElementById('punchLiveTimerDisplay');
    const timerTile = document.getElementById('punchTimerTile');
    const stopwatchIcon = document.getElementById('stopwatchIcon');
    const timerSub = document.getElementById('punchTimerSub');
    const btnPunchIn = document.getElementById('btnPunchIn');
    const btnPunchOut = document.getElementById('btnPunchOut');
    const completedBanner = document.getElementById('punchCompletedBanner');
    const statusBadge = document.getElementById('punchStatusBadge');
    const statusText = document.getElementById('punchStatusText');
    const statusIcon = document.getElementById('punchStatusIcon');
    const punchInDisplay = document.getElementById('punchInDisplay');
    const punchOutDisplay = document.getElementById('punchOutDisplay');
    const punchRemarks = document.getElementById('punchRemarks');
    const existingRemarksBadge = document.getElementById('existingRemarksBadge');
    const remarksText = document.getElementById('remarksText');

    // --- State ---
    let isPunchedIn = station.getAttribute('data-punched-in') === 'true';
    let isPunchedOut = station.getAttribute('data-punched-out') === 'true';
    let checkInTimeRaw = station.getAttribute('data-checkin-raw') || '';
    let checkOutTimeRaw = station.getAttribute('data-checkout-raw') || '';
    let timerInterval = null;

    // --- Live Digital Clock ---
    function updateClock() {
        const now = new Date();
        if (clockEl) {
            clockEl.textContent = now.toLocaleTimeString('en-US', {
                hour: '2-digit',
                minute: '2-digit',
                second: '2-digit',
                hour12: true
            });
        }
        if (dateEl) {
            dateEl.textContent = now.toLocaleDateString('en-US', {
                weekday: 'short',
                month: 'short',
                day: 'numeric',
                year: 'numeric'
            });
        }
    }
    updateClock();
    setInterval(updateClock, 1000);

    // --- Helpers ---
    function formatDuration(totalSeconds) {
        if (totalSeconds < 0) totalSeconds = 0;
        const h = Math.floor(totalSeconds / 3600);
        const m = Math.floor((totalSeconds % 3600) / 60);
        const s = Math.floor(totalSeconds % 60);
        const pad = (n) => String(n).padStart(2, '0');
        return `${pad(h)}h ${pad(m)}m ${pad(s)}s`;
    }

    function parseTodayTime(timeStr) {
        if (!timeStr) return null;
        const parts = timeStr.split(':');
        if (parts.length < 2) return null;
        const d = new Date();
        d.setHours(parseInt(parts[0], 10) || 0);
        d.setMinutes(parseInt(parts[1], 10) || 0);
        d.setSeconds(parts.length > 2 ? (parseInt(parts[2], 10) || 0) : 0);
        d.setMilliseconds(0);
        return d;
    }

    function updateStopwatch() {
        if (!timerDisplay) return;

        if (isPunchedOut && checkInTimeRaw && checkOutTimeRaw) {
            const inD = parseTodayTime(checkInTimeRaw);
            const outD = parseTodayTime(checkOutTimeRaw);
            if (inD && outD) {
                const diff = Math.max(0, Math.floor((outD.getTime() - inD.getTime()) / 1000));
                timerDisplay.textContent = formatDuration(diff);
            }
            return;
        }

        if (isPunchedIn && !isPunchedOut && checkInTimeRaw) {
            const inD = parseTodayTime(checkInTimeRaw);
            if (inD) {
                const now = new Date();
                const diff = Math.max(0, Math.floor((now.getTime() - inD.getTime()) / 1000));
                timerDisplay.textContent = formatDuration(diff);
            }
        } else if (!isPunchedIn) {
            timerDisplay.textContent = "00h 00m 00s";
        }
    }

    function startStopwatch() {
        if (timerInterval) clearInterval(timerInterval);
        updateStopwatch();
        timerInterval = setInterval(updateStopwatch, 1000);
    }

    function stopStopwatch() {
        if (timerInterval) {
            clearInterval(timerInterval);
            timerInterval = null;
        }
        updateStopwatch();
    }

    // Initialize stopwatch based on current state
    if (isPunchedIn && !isPunchedOut) {
        startStopwatch();
    } else if (isPunchedOut) {
        updateStopwatch();
    }

    // --- Toast Notification Helper ---
    function showNotification(msg, isSuccess) {
        let container = document.querySelector('.toast-notification-wrapper');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-notification-wrapper';
            document.body.appendChild(container);
        }

        const alertDiv = document.createElement('div');
        alertDiv.className = 'alert ' + (isSuccess ? 'alert-success' : 'alert-danger') + ' alert-dismissible fade show auto-hide-alert';
        alertDiv.setAttribute('role', 'alert');

        const iconClass = isSuccess ? 'fa-circle-check text-success' : 'fa-triangle-exclamation text-danger';
        alertDiv.innerHTML =
            '<div class="d-flex align-items-center">' +
                '<i class="fa-solid ' + iconClass + ' fs-5 me-2"></i>' +
                '<span>' + msg + '</span>' +
            '</div>' +
            '<button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>';

        container.appendChild(alertDiv);

        setTimeout(function () {
            alertDiv.classList.remove('show');
            alertDiv.style.opacity = '0';
            setTimeout(function () {
                alertDiv.remove();
            }, 400);
        }, 4500);
    }

    function getVerificationToken() {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }

    // --- Expose Global Syncer for Multi-Employee & Form Switching ---
    window.syncPunchStationFromData = function (data) {
        if (!data) return;

        if (data.found) {
            isPunchedIn = Boolean(data.isPunchedIn);
            isPunchedOut = Boolean(data.isPunchedOut);
            checkInTimeRaw = data.checkInRaw || '';
            checkOutTimeRaw = data.checkOutRaw || '';

            station.setAttribute('data-punched-in', isPunchedIn ? 'true' : 'false');
            station.setAttribute('data-punched-out', isPunchedOut ? 'true' : 'false');
            station.setAttribute('data-checkin-raw', checkInTimeRaw);
            station.setAttribute('data-checkout-raw', checkOutTimeRaw);

            if (punchInDisplay) punchInDisplay.textContent = data.checkInDisplay || '--:--';
            if (punchOutDisplay) punchOutDisplay.textContent = data.checkOutDisplay || '--:--';

            if (btnPunchIn) btnPunchIn.style.display = isPunchedIn ? 'none' : 'inline-block';
            if (btnPunchOut) {
                btnPunchOut.style.display = (isPunchedIn && !isPunchedOut) ? 'inline-block' : 'none';
                btnPunchOut.disabled = false;
            }
            if (completedBanner) {
                completedBanner.style.setProperty('display', isPunchedOut ? 'flex' : 'none', 'important');
            }

            if (statusBadge) {
                if (isPunchedOut) {
                    statusBadge.className = 'badge rounded-pill px-3 py-2 fs-7 bg-info text-dark';
                } else if (isPunchedIn) {
                    statusBadge.className = 'badge rounded-pill px-3 py-2 fs-7 bg-success text-white';
                } else {
                    statusBadge.className = 'badge rounded-pill px-3 py-2 fs-7 bg-secondary text-white';
                }
            }
            if (statusText) {
                if (isPunchedOut) statusText.textContent = 'Completed for Today';
                else if (isPunchedIn) statusText.textContent = 'Punched In / Working';
                else statusText.textContent = data.status || 'Not Punched In';
            }
            if (statusIcon) {
                if (isPunchedOut) statusIcon.className = 'fa-solid fa-circle-check me-1';
                else if (isPunchedIn) statusIcon.className = 'fa-solid fa-circle-play me-1';
                else statusIcon.className = 'fa-solid fa-circle-dot me-1';
            }

            if (timerTile) {
                if (isPunchedIn && !isPunchedOut) timerTile.classList.add('active-glow');
                else timerTile.classList.remove('active-glow');
            }
            if (stopwatchIcon) {
                if (isPunchedIn && !isPunchedOut) stopwatchIcon.classList.add('fa-spin');
                else stopwatchIcon.classList.remove('fa-spin');
            }
            if (timerSub) {
                if (isPunchedOut) timerSub.textContent = 'Shift Completed';
                else if (isPunchedIn) timerSub.textContent = 'Active Working';
                else timerSub.textContent = 'Not Started';
            }

            if (isPunchedIn && !isPunchedOut) {
                startStopwatch();
            } else if (isPunchedOut) {
                stopStopwatch();
                if (timerDisplay) {
                    timerDisplay.textContent = data.duration || timerDisplay.textContent;
                }
            } else {
                stopStopwatch();
                if (timerDisplay) timerDisplay.textContent = '00h 00m 00s';
            }
        } else {
            // Record not found - Reset to blank state cleanly
            isPunchedIn = false;
            isPunchedOut = false;
            checkInTimeRaw = '';
            checkOutTimeRaw = '';

            station.setAttribute('data-punched-in', 'false');
            station.setAttribute('data-punched-out', 'false');
            station.setAttribute('data-checkin-raw', '');
            station.setAttribute('data-checkout-raw', '');

            if (punchInDisplay) punchInDisplay.textContent = '--:--';
            if (punchOutDisplay) punchOutDisplay.textContent = '--:--';

            if (btnPunchIn) {
                btnPunchIn.style.display = 'inline-block';
                btnPunchIn.disabled = false;
            }
            if (btnPunchOut) btnPunchOut.style.display = 'none';
            if (completedBanner) completedBanner.style.setProperty('display', 'none', 'important');

            if (statusBadge) statusBadge.className = 'badge rounded-pill px-3 py-2 fs-7 bg-secondary text-white';
            if (statusText) statusText.textContent = 'Not Punched In';
            if (statusIcon) statusIcon.className = 'fa-solid fa-circle-dot me-1';

            if (timerTile) timerTile.classList.remove('active-glow');
            if (stopwatchIcon) stopwatchIcon.classList.remove('fa-spin');
            if (timerSub) timerSub.textContent = 'Not Started';

            stopStopwatch();
            if (timerDisplay) timerDisplay.textContent = '00h 00m 00s';
        }
    };

    // --- 1-Click Punch In Action ---
    if (btnPunchIn) {
        btnPunchIn.addEventListener('click', async function () {
            const remarksVal = punchRemarks ? punchRemarks.value : '';
            btnPunchIn.disabled = true;
            const originalHtml = btnPunchIn.innerHTML;
            btnPunchIn.innerHTML = '<i class="fa-solid fa-spinner fa-spin fs-4 me-2"></i> Punching In...';

            try {
                const formData = new URLSearchParams();
                formData.append('__RequestVerificationToken', getVerificationToken());
                formData.append('remarks', remarksVal);

                const empSelect = document.getElementById('selectEmployeeId');
                if (empSelect && empSelect.value) {
                    formData.append('employeeId', empSelect.value);
                }

                const response = await fetch('/Attendance/PunchIn', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: formData.toString()
                });

                const data = await response.json();

                if (data.success) {
                    isPunchedIn = true;
                    isPunchedOut = false;
                    checkInTimeRaw = data.checkInRaw || new Date().toTimeString().split(' ')[0];

                    if (punchInDisplay) punchInDisplay.textContent = data.checkIn;
                    
                    // Also sync with form input if present on Mark Attendance page
                    const formCheckIn = document.getElementById('formCheckInInput');
                    if (formCheckIn && data.checkInTime24) {
                        formCheckIn.value = data.checkInTime24;
                    }
                    const checkInPreview = document.getElementById('checkInPreviewText');
                    if (checkInPreview && data.checkIn) {
                        checkInPreview.textContent = data.checkIn;
                    }

                    btnPunchIn.style.display = 'none';
                    if (btnPunchOut) {
                        btnPunchOut.style.display = 'inline-block';
                        btnPunchOut.disabled = false;
                    }

                    if (statusBadge) {
                        statusBadge.className = 'badge rounded-pill px-3 py-2 fs-7 bg-success text-white';
                    }
                    if (statusText) statusText.textContent = 'Punched In / Working';
                    if (statusIcon) statusIcon.className = 'fa-solid fa-circle-play me-1';

                    if (timerTile) timerTile.classList.add('active-glow');
                    if (stopwatchIcon) stopwatchIcon.classList.add('fa-spin');
                    if (timerSub) timerSub.textContent = 'Active Working';

                    if (existingRemarksBadge && remarksText && remarksVal) {
                        remarksText.textContent = remarksVal;
                        existingRemarksBadge.style.display = 'inline-block';
                    }

                    startStopwatch();
                    showNotification(data.message || 'Punched in successfully!', true);
                } else {
                    btnPunchIn.disabled = false;
                    btnPunchIn.innerHTML = originalHtml;
                    showNotification(data.message || 'Could not punch in.', false);
                }
            } catch (err) {
                console.error('Punch in error:', err);
                btnPunchIn.disabled = false;
                btnPunchIn.innerHTML = originalHtml;
                showNotification('Network error occurred while punching in.', false);
            }
        });
    }

    // --- 1-Click Punch Out Action ---
    if (btnPunchOut) {
        btnPunchOut.addEventListener('click', async function () {
            if (!confirm('Are you sure you want to Punch Out and finish your shift for today?')) {
                return;
            }

            const remarksVal = punchRemarks ? punchRemarks.value : '';
            btnPunchOut.disabled = true;
            const originalHtml = btnPunchOut.innerHTML;
            btnPunchOut.innerHTML = '<i class="fa-solid fa-spinner fa-spin fs-4 me-2"></i> Punching Out...';

            try {
                const formData = new URLSearchParams();
                formData.append('__RequestVerificationToken', getVerificationToken());
                formData.append('remarks', remarksVal);

                const empSelect = document.getElementById('selectEmployeeId');
                if (empSelect && empSelect.value) {
                    formData.append('employeeId', empSelect.value);
                }

                const response = await fetch('/Attendance/PunchOut', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: formData.toString()
                });

                const data = await response.json();

                if (data.success) {
                    isPunchedOut = true;
                    checkOutTimeRaw = data.checkOutRaw || new Date().toTimeString().split(' ')[0];

                    if (punchOutDisplay) punchOutDisplay.textContent = data.checkOut;

                    // Also sync with form input if present on Mark Attendance page
                    const formCheckOut = document.getElementById('formCheckOutInput');
                    if (formCheckOut && data.checkOutTime24) {
                        formCheckOut.value = data.checkOutTime24;
                    }
                    const checkOutPreview = document.getElementById('checkOutPreviewText');
                    if (checkOutPreview && data.checkOut) {
                        checkOutPreview.textContent = data.checkOut;
                    }

                    btnPunchOut.style.display = 'none';
                    if (completedBanner) completedBanner.style.setProperty('display', 'flex', 'important');

                    if (statusBadge) {
                        statusBadge.className = 'badge rounded-pill px-3 py-2 fs-7 bg-info text-dark';
                    }
                    if (statusText) statusText.textContent = 'Completed for Today';
                    if (statusIcon) statusIcon.className = 'fa-solid fa-circle-check me-1';

                    if (timerTile) timerTile.classList.remove('active-glow');
                    if (stopwatchIcon) stopwatchIcon.classList.remove('fa-spin');
                    if (timerSub) timerSub.textContent = 'Shift Completed';

                    stopStopwatch();
                    if (data.duration && timerDisplay) {
                        timerDisplay.textContent = data.duration;
                    }
                    showNotification(data.message || 'Punched out successfully!', true);
                } else {
                    btnPunchOut.disabled = false;
                    btnPunchOut.innerHTML = originalHtml;
                    showNotification(data.message || 'Could not punch out.', false);
                }
            } catch (err) {
                console.error('Punch out error:', err);
                btnPunchOut.disabled = false;
                btnPunchOut.innerHTML = originalHtml;
                showNotification('Network error occurred while punching out.', false);
            }
        });
    }
});
