document.addEventListener('DOMContentLoaded', () => {
    const actionButtons = document.querySelectorAll('.action-btn');
    const overlay = document.getElementById('blackScreenTransition');
    const soundBtn = document.querySelector('.sound-btn');

    
    const bgMusic = new Audio('/audio/startviewaudio.mp3');
    bgMusic.loop = true;
    bgMusic.volume = 0.5;     
    bgMusic.muted = false;   

    const buttonHoverSound = new Audio('/audio/buttonhover.mp3');
    buttonHoverSound.volume = 1;

    const buttonClickSound = new Audio('/audio/buttonclick.mp3');
    buttonClickSound.volume = 1;

    
    soundBtn.style.backgroundColor = "#5c0000";
    soundBtn.style.borderColor = "#ff3333";

    let audioStarted = false;
    const startAudio = () => {
        if (!audioStarted) {
            bgMusic.play().then(() => {
                console.log("Muzyka gra poprawnie (odciszona).");
            }).catch(e => {
                console.log("Przeglądarka zablokowała autoodtwarzanie. Ruszy po kliknięciu.");
            });
            audioStarted = true;
        }
    };

    document.body.addEventListener('click', startAudio, { once: true });

 
    soundBtn.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();

        if (bgMusic.paused) {
            bgMusic.play();
            bgMusic.muted = false;
            soundBtn.style.backgroundColor = "#5c0000"; 
            soundBtn.style.opacity = "1";
        } else {
            bgMusic.pause();
            soundBtn.style.backgroundColor = "#110000";
            soundBtn.style.opacity = "0.6";
        }
    });

   
    function fadeOutAudio(duration) {
        const step = 50;
        const volStep = bgMusic.volume / (duration / step);

        const fadeInterval = setInterval(() => {
            if (bgMusic.volume > volStep) {
                bgMusic.volume -= volStep;
            } else {
                bgMusic.volume = 0;
                bgMusic.pause();
                clearInterval(fadeInterval);
            }
        }, step);
    }


    actionButtons.forEach(button => {
        // Hover sound
        button.addEventListener('mouseenter', () => {
            buttonHoverSound.currentTime = 0; // restart od początku przy każdym hover
            buttonHoverSound.play().catch(() => {}); // ignoruj błędy autoodtwarzania
        });

        button.addEventListener('click', function (e) {
            e.preventDefault();
            const targetUrl = this.getAttribute('data-url');
            const isDemoBtn = this.classList.contains('demo-btn');

            // If demo button, check status and start demo
            if (isDemoBtn) {
                fetch('/Demo/CheckStatus', {
                    method: 'POST'
                })
                    .then(r => r.json())
                    .then(data => {
                        if (!data.canPlay) {
                            showDemoAlert(data.message);
                            return;
                        }

                        // Start demo session
                        fetch('/Demo/StartDemo', {
                            method: 'POST'
                        })
                            .then(r => r.json())
                            .then(demoData => {
                                if (demoData.success) {
                                    proceedToPage(this, '/Account/SelectWeapon', bgMusic, overlay, buttonClickSound);
                                } else {
                                    showDemoAlert(demoData.error || 'Failed to start demo');
                                }
                            })
                            .catch(err => {
                                console.error('Error starting demo:', err);
                                showDemoAlert('Error starting demo session');
                            });
                    })
                    .catch(err => {
                        console.error('Error checking demo status:', err);
                        showDemoAlert('Error checking demo status');
                    });
            } else {
                proceedToPage(this, targetUrl, bgMusic, overlay, buttonClickSound);
            }
        });
    });

    function proceedToPage(button, targetUrl, bgMusic, overlay, clickSound) {
        button.classList.add('btn-fall');

        if (!bgMusic.paused) {
            fadeOutAudio(900);
        }

        setTimeout(() => {
            overlay.classList.add('active');
        }, 450);

        clickSound.currentTime = 0;
        clickSound.play().catch(() => { });

        setTimeout(() => {
            if (targetUrl) {
                window.location.href = targetUrl;
            }
        }, 950);
    }

    function showDemoAlert(message) {
        const alertContainer = document.createElement('div');
        alertContainer.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: linear-gradient(135deg, #2d0000 0%, #1a0000 100%);
            border: 3px solid #c41e3a;
            border-radius: 15px;
            padding: 30px;
            max-width: 400px;
            text-align: center;
            color: #ecf0f1;
            z-index: 1000;
            box-shadow: 0 0 30px rgba(196, 30, 58, 0.8);
            animation: slideDown 0.3s ease-out;
        `;

        alertContainer.innerHTML = `
            <h2 style="color: #ff6b6b; margin-bottom: 15px; margin-top: 0;">🔒 Informacja</h2>
            <p style="margin: 0 0 20px 0; font-size: 1.1rem;">${message}</p>
            <button id="close-demo-alert" style="
                background: #c41e3a;
                color: white;
                border: 2px solid #ff6b6b;
                padding: 10px 20px;
                border-radius: 6px;
                font-size: 1rem;
                cursor: pointer;
                transition: all 0.3s ease;
            ">Powrót</button>
        `;

        document.body.appendChild(alertContainer);

        document.getElementById('close-demo-alert').addEventListener('click', () => {
            alertContainer.style.animation = 'slideUp 0.3s ease-out forwards';
            setTimeout(() => alertContainer.remove(), 300);
        });
    }
});