document.addEventListener('DOMContentLoaded', () => {
    const loginMusic = new Audio('/audio/startviewaudio.mp3');
    loginMusic.loop = true;
    loginMusic.volume = 0.5;

    const buttonhover = new Audio('/audio/buttonhover.mp3');
    buttonhover.volume = 1;

    const buttonclick = new Audio('/audio/buttonclick.mp3');
    buttonclick.volume = 1;
});
