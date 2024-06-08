window.blazorGestureDetected = function (dotnetHelper) {
    let touchstartX = 0;
    let touchstartY = 0;
    let touchendX = 0;
    let touchendY = 0;

    document.addEventListener('touchstart', function (event) {
        touchstartX = event.changedTouches[0].screenX;
        touchstartY = event.changedTouches[0].screenY;

        const deltaY = touchendY - touchstartY;
        // there is some bug need to fix

        if (Math.abs(deltaY) > 2) { 
                event.preventDefault(); 
        }
    }, { passive: false });

    document.addEventListener('touchend', function (event) {
        touchendX = event.changedTouches[0].screenX;
        touchendY = event.changedTouches[0].screenY;
        handleGesture();
    }, false);

    function handleGesture() {
        const swipeThreshold = 50; // Minimum distance for a swipe to be considered
        const deltaX = touchendX - touchstartX;
        const deltaY = touchendY - touchstartY;

        if (Math.abs(deltaX) > Math.abs(deltaY)) {
            // Horizontal swipe
            if (Math.abs(deltaX) > swipeThreshold) {
                if (deltaX > 0) {
                    dotnetHelper.invokeMethodAsync('OnArrowKeyPressed', 'ARROWRIGHT');
                } else {
                    dotnetHelper.invokeMethodAsync('OnArrowKeyPressed', 'ARROWLEFT');
                }
            }
        } else {
            // Vertical swipe
            if (Math.abs(deltaY) > swipeThreshold) {
                if (deltaY > 0) {
                    dotnetHelper.invokeMethodAsync('OnArrowKeyPressed', 'ARROWDOWN');
                } else {
                    dotnetHelper.invokeMethodAsync('OnArrowKeyPressed', 'ARROWUP');
                }
            }
        }
    }
};
