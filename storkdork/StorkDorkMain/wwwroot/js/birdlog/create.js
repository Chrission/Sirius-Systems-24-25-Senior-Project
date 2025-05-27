document.addEventListener('DOMContentLoaded', function() {
    // Bird Search Elements
    const birdSearch = document.getElementById('birdSearch');
    const birdResults = document.getElementById('birdResults');
    const birdError = document.getElementById('birdError');
    const commonNameDisplay = document.getElementById('commonNameDisplay');
    const scientificNameDisplay = document.getElementById('scientificNameDisplay');
    const birdIdInput = document.getElementById('BirdId');

    // Debounce function
    const debounce = (func, delay) => {
        let timeout;
        return (...args) => {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), delay);
        };
    };

    // Bird search handler
    const handleBirdSearch = debounce(async (e) => {
        const term = e.target.value.trim();
        birdError.style.display = 'none';

        if (term.length < 2) {
            birdResults.innerHTML = '';
            birdResults.style.display = 'none';
            clearBirdSelection();
            return;
        }

        try {
            const response = await fetch(`/birds/search?term=${encodeURIComponent(term)}`);
            if (!response.ok) throw new Error('Search failed');
            
            const birds = await response.json();
            displayBirdResults(birds);
        } catch (error) {
            showBirdError('Failed to search species. Please try again.');
            clearBirdSelection();
        }
    }, 300);

    // Connect input handler
    birdSearch.addEventListener('input', handleBirdSearch);

    // Display results
    function displayBirdResults(birds) {
        birdResults.innerHTML = '';
        
        if (birds.length === 0) {
            birdResults.innerHTML = '<div class="result-item">No matching species found</div>';
            birdResults.style.display = 'block';
            return;
        }

        birds.forEach(bird => {
            const div = document.createElement('div');
            div.className = 'result-item';
            div.innerHTML = `
                <div class="bird-name">${bird.commonName}</div>
                <div class="scientific-name">${bird.scientificName}</div>
            `;
            div.addEventListener('click', () => selectBird(bird));
            birdResults.appendChild(div);
        });
        birdResults.style.display = 'block';
    }

    // Selection handler
    function selectBird(bird) {
        birdSearch.value = bird.commonName;
        commonNameDisplay.textContent = bird.commonName;
        scientificNameDisplay.textContent = bird.scientificName;
        birdIdInput.value = bird.id;
        birdResults.style.display = 'none';
    }

    // Clear selection
    function clearBirdSelection() {
        birdIdInput.value = '';
        commonNameDisplay.textContent = '-';
        scientificNameDisplay.textContent = '-';
    }

    // Error handling
    function showBirdError(message) {
        birdError.textContent = message;
        birdError.style.display = 'block';
        birdResults.style.display = 'none';
    }



    function initializeBirdLogForm() {
        const form = document.getElementById('sightingForm');
        if (!form) return;

        let createMap = null;
        let createMarker = null;

        // Image upload handling
        const photoFile = document.getElementById('photoFile');
        const photoPreviewContainer = document.getElementById('photoPreviewContainer');
        const photoPreview = document.getElementById('photoPreview');

        if (photoFile && photoPreviewContainer && photoPreview) {
            photoFile.addEventListener('change', function(e) {
                const file = e.target.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = function(e) {
                        photoPreview.src = e.target.result;
                        photoPreviewContainer.style.display = 'block';
                    }
                    reader.readAsDataURL(file);
                }
            });
        }

        // Map initialization
        function initCreateMap() {
            try {
                if (createMap) createMap.remove();
                
                const initialLat = parseFloat(document.getElementById('LatitudeInput').value) || 44.8485;
                const initialLng = parseFloat(document.getElementById('LongitudeInput').value) || -123.2340;

                createMap = L.map('map').setView([initialLat, initialLng], 14);
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    maxZoom: 19,
                    attribution: '© OpenStreetMap contributors'
                }).addTo(createMap);

                createMap.on('click', function(e) {
                    updateLocation(e.latlng);
                });

                if (initialLat && initialLng) {
                    const initialCoords = L.latLng(initialLat, initialLng);
                    updateLocation(initialCoords);
                }

            } catch (error) {
                console.error('Map initialization error:', error);
                const mapDiv = document.getElementById('map');
                if (mapDiv) {
                    mapDiv.innerHTML = '<div class="alert alert-danger">Error loading map. Refresh page.</div>';
                }
            }
        }

        // Location update handler
        function updateLocation(latlng, displayName) {
            if (!createMap) return;

            if (createMarker) createMap.removeLayer(createMarker);
            createMarker = L.marker(latlng).addTo(createMap);
            
            document.getElementById('LatitudeInput').value = latlng.lat;
            document.getElementById('LongitudeInput').value = latlng.lng;
            
            const locationSearch = document.getElementById('locationSearch');
            if (locationSearch) {
                if (displayName) {
                    locationSearch.value = displayName;
                } else {
                    fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${latlng.lat}&lon=${latlng.lng}`, {
                        headers: { 'User-Agent': 'StorkDork/1.0' }
                    })
                    .then(response => response.json())
                    .then(data => {
                        locationSearch.value = data.display_name || 'Selected location';
                    })
                    .catch(console.error);
                }
            }

            document.getElementById('latDisplay').textContent = latlng.lat.toFixed(4);
            document.getElementById('lngDisplay').textContent = latlng.lng.toFixed(4);
            document.getElementById('locationDisplay').style.display = 'block';
        }

        // Location search functionality
        const locationSearch = document.getElementById('locationSearch');
        const locationResults = document.getElementById('locationSearchResults');
        let searchTimeout;

        if (locationSearch && locationResults) {
            locationSearch.addEventListener('input', function(e) {
                const query = e.target.value.trim();
                clearTimeout(searchTimeout);
                
                if (query.length < 3) {
                    locationResults.style.display = 'none';
                    return;
                }

                searchTimeout = setTimeout(async () => {
                    try {
                        const response = await fetch(
                            `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(query)}&format=json&limit=5`,
                            { headers: { 'User-Agent': 'StorkDork/1.0' } }
                        );
                        
                        const results = await response.json();
                        locationResults.innerHTML = '';
                        
                        if (results.length === 0) {
                            locationResults.innerHTML = '<div class="location-result">No results found</div>';
                            locationResults.style.display = 'block';
                            return;
                        }
                        
                        results.forEach(result => {
                            const div = document.createElement('div');
                            div.className = 'location-result';
                            div.innerHTML = `
                                <div class="fw-bold">${result.display_name}</div>
                                <small>Lat: ${result.lat}, Lng: ${result.lon}</small>
                            `;
                            div.addEventListener('click', () => {
                                const latLng = L.latLng(result.lat, result.lon);
                                createMap.setView(latLng, 14);
                                updateLocation(latLng, result.display_name);
                                locationResults.style.display = 'none';
                            });
                            locationResults.appendChild(div);
                        });
                        
                        locationResults.style.display = 'block';
                    } catch (error) {
                        console.error('Location search error:', error);
                        locationResults.innerHTML = '<div class="location-result">Error fetching results</div>';
                        locationResults.style.display = 'block';
                    }
                }, 500);
            });
        }

        // Clear location handler
        const clearLocation = document.getElementById('clearLocation');
        if (clearLocation) {
            clearLocation.addEventListener('click', function() {
                if (createMap && createMarker) createMap.removeLayer(createMarker);
                document.getElementById('LatitudeInput').value = '';
                document.getElementById('LongitudeInput').value = '';
                document.getElementById('locationSearch').value = '';
                document.getElementById('locationDisplay').style.display = 'none';
            });
        }

        // Form validation
        form.addEventListener('submit', function (event) {
            document.getElementById('birdError').style.display = 'none';
            document.getElementById('locationError').style.display = 'none';

            const birdId = document.getElementById('BirdId').value;
            const latitude = document.getElementById('LatitudeInput').value;
            const longitude = document.getElementById('LongitudeInput').value;
            let isValid = true;

            if (!birdId) {
                isValid = false;
                birdError.textContent = 'Please select a bird species';
                birdError.style.display = 'block';
            }

            if (!latitude || !longitude) {
                isValid = false;
                document.getElementById('locationError').textContent = 'Please select a location on the map';
                document.getElementById('locationError').style.display = 'block';
            }

            if (!isValid) {
                event.preventDefault();
                scrollToFirstError();
            }
        });

        // Initialize map
        setTimeout(initCreateMap, 100);
    }

    // Initialize form functionality
    if (typeof L !== 'undefined') {
        initializeBirdLogForm();
    } else {
        console.error('Leaflet not loaded!');
        const mapDiv = document.getElementById('map');
        if (mapDiv) {
            mapDiv.innerHTML = '<div class="alert alert-danger">Error: Map library failed to load. Refresh the page.</div>';
        }
    }

    // Event listeners for closing dropdowns
    document.addEventListener('click', function(event) {
        if (!event.target.closest('#birdSearch, #birdResults')) {
            birdResults.style.display = 'none';
        }
        if (!event.target.closest('#locationSearch, #locationSearchResults')) {
            document.getElementById('locationSearchResults').style.display = 'none';
        }
    });
    async function refreshMapMarkers() {
        try {
            const response = await fetch('/api/map/GetSightings');
            const sightings = await response.json();
            
            // Clear existing markers
            if(window.markers) {
                window.markers.forEach(marker => marker.remove());
            }
            
            // Add new markers
            window.markers = sightings.map(sighting => {
                return L.marker([sighting.latitude, sighting.longitude])
                    .bindPopup(`
                        <b>${sighting.commonName}</b><br>
                        ${sighting.description}<br>
                        <small>${new Date(sighting.date).toLocaleDateString()}</small>
                    `);
            }).forEach(m => m.addTo(createMap));
        } catch (error) {
            console.error('Error refreshing markers:', error);
        }
    }
    
    
    form.addEventListener('submit', async function (event) {
    
        
        if (isValid) {
    
            event.preventDefault();
            
            try {
                const formData = new FormData(form);
    
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                
                const response = await fetch(form.action, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'RequestVerificationToken': token
                    }
                });
    
                if (response.ok) {
                    await refreshMapMarkers();
                    // Show success message
                    const confirmation = await response.json();
                    window.location.href = `/BirdLog/Confirmation?userId=${confirmation.userId}`;
                } else {
                    throw new Error('Submission failed');
                }
            } catch (error) {
                console.error('Submission error:', error);
                showBirdError('Failed to save sighting. Please try again.');
            }
        }
    });
    
    if (createMap) {
        refreshMapMarkers();
    }
    
});

function scrollToFirstError() {
    const errors = document.querySelectorAll('.text-danger');
    const navbar = document.querySelector('.navbar');
    const navbarHeight = navbar ? navbar.offsetHeight : 0;
    
    const firstVisibleError = Array.from(errors).find(error => 
        window.getComputedStyle(error).display === 'block'
    );

    if (firstVisibleError) {
        const errorPosition = firstVisibleError.getBoundingClientRect().top + window.pageYOffset;
        const offsetPosition = errorPosition - navbarHeight - 20;
        window.scrollTo({ top: offsetPosition, behavior: 'smooth' });
    }
}

