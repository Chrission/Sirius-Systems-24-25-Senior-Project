// Truncation helper function
function truncateText(text, maxLength = 40) {
    return text.length > maxLength ? text.substring(0, maxLength) + "..." : text;
}

// Caching layer
const geocache = {
    get: (key) => {
        try {
            const cache = JSON.parse(localStorage.getItem('geocache') || '{}');
            return cache[key];
        } catch {
            return null;
        }
    },
    set: (key, value) => {
        try {
            const cache = JSON.parse(localStorage.getItem('geocache') || '{}');
            cache[key] = value;
            localStorage.setItem('geocache', JSON.stringify(cache));
        } catch (e) {
            console.error('Geocache error:', e);
        }
    }
};

document.addEventListener("DOMContentLoaded", function () {
    // Filter panel toggle
    const filterButton = document.getElementById("openFilterMenu");
    const filterPanel = document.getElementById("filterPanel");
    if (filterButton && filterPanel) {
        filterButton.addEventListener("click", function () {
            filterPanel.style.display = filterPanel.style.display === "none" ? "block" : "none";
        });
    }

    // Filter form functionality
    const overlay = document.getElementById("filterOverlay");
    const openFilterFormButton = document.getElementById("openFilterForm");
    const closeFilterFormButton = document.getElementById("closeFilterForm");
    const birdSearch = document.getElementById("birdSearch");
    const birdDropdown = document.getElementById("birdDropdown");
    const selectedBirdsContainer = document.getElementById("selectedBirdsContainer");
    const selectedBirdsInput = document.getElementById("selectedBirds");
    const filterForm = document.getElementById("filterForm");
    const locationFilterInput = document.getElementById('locationFilter');

    let selectedBirds = [];
    const allBirdNames = window.allBirdNames || [];

    // Bird filter functionality
    if (birdSearch && birdDropdown) {
        function updateDropdown(searchTerm) {
            birdDropdown.innerHTML = "";
            const matchingBirds = allBirdNames.filter(bird => 
                bird.toLowerCase().includes(searchTerm.toLowerCase())
            );
            
            matchingBirds.forEach(bird => {
                const dropdownItem = document.createElement("div");
                dropdownItem.className = "dropdown-item";
                dropdownItem.textContent = bird;
                dropdownItem.style.cursor = "pointer";
                dropdownItem.addEventListener("click", function () {
                    if (!selectedBirds.includes(bird)) {
                        selectedBirds.push(bird);
                        updateSelectedBirds();
                    }
                });
                birdDropdown.appendChild(dropdownItem);
            });
            
            birdDropdown.style.display = matchingBirds.length > 0 ? "block" : "none";
        }

        if (locationFilterInput) {
            locationFilterInput.addEventListener('input', function(e) {
                const searchTerm = e.target.value.toLowerCase();
                document.querySelectorAll('.location-name').forEach(container => {
                    const locationName = container.textContent.toLowerCase();
                    const row = container.closest('tr');
                    row.style.display = locationName.includes(searchTerm) ? '' : 'none';
                });
            });
        }

        function updateSelectedBirds() {
            selectedBirdsContainer.innerHTML = "";
            selectedBirds.forEach(bird => {
                const chip = document.createElement("div");
                chip.className = "badge bg-primary me-2";
                chip.innerHTML = `
                    ${bird}
                    <span class="ms-2" style="cursor: pointer;" onclick="removeBird('${bird}')">×</span>
                `;
                selectedBirdsContainer.appendChild(chip);
            });
            selectedBirdsInput.value = selectedBirds.join(",");
        }

        window.removeBird = function (bird) {
            selectedBirds = selectedBirds.filter(b => b !== bird);
            updateSelectedBirds();
        };

        birdSearch.addEventListener("input", function () {
            updateDropdown(this.value);
        });
    }

    // Filter overlay controls
    if (openFilterFormButton && closeFilterFormButton) {
        openFilterFormButton.addEventListener("click", function () {
            overlay.classList.remove("d-none");
        });

        closeFilterFormButton.addEventListener("click", function () {
            overlay.classList.add("d-none");
        });
    }

    // Form submission
    if (filterForm) {
        filterForm.addEventListener("submit", function (event) {
            event.preventDefault();
            window.location.href = `/BirdLog/Index?selectedBirds=${selectedBirds.join(",")}`;
        });
    }
    

    // Optimized location geocoding
    const locationDisplays = Array.from(document.querySelectorAll('.location-display'));
    
    locationDisplays.forEach((element, index) => {
        setTimeout(() => {
            const lat = element.dataset.lat;
            const lng = element.dataset.lng;
            const cacheKey = `${lat},${lng}`;
            const cached = geocache.get(cacheKey);
            
            const nameSpan = element.querySelector('.location-name');
            const spinner = element.querySelector('.loading-spinner');
            const coordContainer = element.querySelector('.coordinates-container');

            if (!lat || !lng) return;

            // Show cached results immediately
            if (cached) {
                if (cached.error) {
                    showError();
                } else {
                    showResult(cached.name);
                }
                return;
            }

            fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}`, {
                headers: { 'User-Agent': 'StorkDork/1.0 (contact@yourdomain.com)' }
            })
            .then(response => response.json())
            .then(data => {
                const name = data.display_name || 'Unnamed location';
                geocache.set(cacheKey, { name });
                showResult(name);
            })
            .catch(error => {
                console.error('Geocoding error:', error);
                geocache.set(cacheKey, { error: true });
                showError();
            });

            function showResult(name) {
                if (coordContainer) coordContainer.style.display = 'none';
                if (spinner) spinner.remove();
                if (nameSpan) {
                    nameSpan.textContent = truncateText(name);
                    nameSpan.style.display = 'block';
                    // Store the full location name for searching
                    element.setAttribute('data-full-location', name.toLowerCase());
                }
            }

            function showError() {
                if (spinner) spinner.remove();
                if (coordContainer) coordContainer.style.opacity = '0.5';
                if (nameSpan) {
                    nameSpan.textContent = truncateText(`${lat}, ${lng}`);
                    nameSpan.classList.add('text-muted');
                    nameSpan.style.display = 'block';
                    element.setAttribute('data-full-location', `${lat}, ${lng}`);
                }
            }

        }, index * 500); // Stagger requests by 500ms
    });

    // Click outside handler
    document.addEventListener("click", function (event) {
        if (birdSearch && !birdSearch.contains(event.target)) {
            birdDropdown.style.display = "none";
        }
    });
});

document.addEventListener("DOMContentLoaded", function () {
    // Global search functionality
    const globalSearch = document.getElementById('globalSearch');
    const clearSearchBtn = document.getElementById('clearSearch');
    let searchTimeout;

    function performSearch() {
        const searchTerm = globalSearch.value.trim().toLowerCase();
        const hasSearch = searchTerm.length >= 2;
        
        // Show/hide clear button
        clearSearchBtn.style.display = hasSearch ? 'block' : 'none';

        // Try to parse the search term as a date
        let parsedDate = null;
        if (hasSearch) {
            const tryDate = new Date(searchTerm);
            if (!isNaN(tryDate)) {
                parsedDate = tryDate;
            }
        }

        // Get all table rows (skip header row)
        const rows = document.querySelectorAll('tbody tr');
        
        rows.forEach(row => {
            const birdCell = row.children[1].textContent.toLowerCase();
            // Use the full geocoded location for searching
            const locationCell = row.querySelector('.location-display')?.getAttribute('data-full-location') || '';
            const dateCell = row.children[0].textContent.toLowerCase();

            let match = false;

            // Match bird or location as before
            if (birdCell.includes(searchTerm) || locationCell.includes(searchTerm)) {
                match = true;
            }

            // Match date column (supports "may 10 2025", "5/10/2025", "2025-05-10", etc.)
            if (!match && hasSearch) {
                // Try direct string match
                if (dateCell.includes(searchTerm)) {
                    match = true;
                } else if (parsedDate) {
                    // Try to match formatted date
                    const dateVariants = [
                        parsedDate.toLocaleDateString('en-US'), // e.g. 5/10/2025
                        parsedDate.toLocaleDateString('en-GB'), // e.g. 10/05/2025
                        parsedDate.toISOString().slice(0, 10),  // e.g. 2025-05-10
                        parsedDate.toDateString().toLowerCase() // e.g. "sat may 10 2025"
                    ];
                    for (const variant of dateVariants) {
                        if (dateCell.includes(variant.toLowerCase())) {
                            match = true;
                            break;
                        }
                    }
                }
            }

            row.style.display = match || !hasSearch ? '' : 'none';
        });
    }

    globalSearch.addEventListener('input', function(e) {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(performSearch, 300);
    });

    clearSearchBtn.addEventListener('click', function() {
        globalSearch.value = '';
        performSearch();
    });

    // Listen for location name updates
    document.addEventListener('locationNameUpdated', performSearch);

    // Modified location geocoding code to dispatch events
    // (not used anymore, but left for reference)
    // function showResult(name) {
    //     if (nameSpan) {
    //         nameSpan.textContent = truncateText(name);
    //         nameSpan.style.display = 'block';
    //         nameSpan.dispatchEvent(new Event('locationNameUpdated'));
    //     }
    // }
});