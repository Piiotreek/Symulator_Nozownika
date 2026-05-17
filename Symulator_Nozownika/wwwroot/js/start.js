document.addEventListener('DOMContentLoaded', () => {
    const actionButtons = document.querySelectorAll('.action-btn');
    const overlay = document.getElementById('blackScreenTransition');
    const soundBtn = document.querySelector('.sound-btn');

    
    const bgMusic = new Audio('/audio/startviewaudio.mp3');
    bgMusic.loop = true;
    bgMusic.volume = 0.5;     
    bgMusic.muted = false;   

    
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
        button.addEventListener('click', function (e) {
            e.preventDefault();
            const targetUrl = this.getAttribute('data-url');

            this.classList.add('btn-fall');

            if (!bgMusic.paused) {
                fadeOutAudio(900);
            }

            setTimeout(() => {
                overlay.classList.add('active');
            }, 450);

            setTimeout(() => {
                if (targetUrl) {
                    window.location.href = targetUrl;
                }
            }, 950);
        });
    });
});