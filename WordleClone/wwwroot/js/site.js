// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function flipTilesSequentially(rowIndex) {
    var row = document.querySelectorAll('.wordle-grid .row')[rowIndex];
    var tiles = row.querySelectorAll('.tile');
    var delay = 100;  // initial delay

    tiles.forEach(tile => {
        setTimeout(() => {
            tile.style.transform = 'rotateY(180deg)';
            
            // Add an event listener to handle the middle of the transition
            tile.addEventListener('transitionend', function handleFlip() {
                var tileFront = tile.querySelector('.tile-front');
                var tileBack = tile.querySelector('.tile-back');

                if (tile.style.transform === 'rotateY(180deg)') {
                    tileFront.style.zIndex = 1;
                    tileBack.style.zIndex = 2;
                } else {
                    tileFront.style.zIndex = 2;
                    tileBack.style.zIndex = 1;
                }

                // Remove the event listener after it's executed to prevent multiple bindings
                tile.removeEventListener('transitionend', handleFlip);
            });

        }, delay);

        delay += 350;  // delay for the next tile
    });
}




console.log("js loaded");