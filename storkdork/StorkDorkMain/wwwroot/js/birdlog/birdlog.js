
//
// wwwroot/js/birdlog/birdlog.js

//CREATE.CSHTML JAVASCRIPT
// Function to initialize the form validation and event listeners
// wwwroot/js/birdlog/birdlog.js

// Function to initialize the form validation and event listeners
function initializeBirdLogForm() {
    const form = document.getElementById('sightingForm');
    if (!form) return; // Exit if the form doesn't exist on the page

    // Add event listener for form submission
    form.addEventListener('submit', function (event) {
        // Get the selected bird and location values
        const birdId = document.getElementById('BirdId').value;
        const location = document.getElementById('PnwLocation').value;

        // Get the error message boxes
        const birdError = document.getElementById('birdError');
        const locationError = document.getElementById('locationError');

        // Reset any previous error messages
        birdError.textContent = "";
        locationError.textContent = "";

        // Assume the form is valid unless we find a problem
        let isValid = true;

        // Check if both bird and location are left blank (not even N/A)
        if (birdId === '' && location === '') {
            // If both are left blank, show an error
            event.preventDefault(); // Stop the form from submitting
            birdError.textContent = "Please select a bird or choose N/A."; // Show error message for bird
            locationError.textContent = "Please select a location or choose N/A."; // Show error message for location
            isValid = false; // Mark the form as invalid

            // Scroll to the first error message
            scrollToFirstError();
        } else if (birdId === '') {
            // If bird is left blank (not even N/A), show an error
            event.preventDefault();
            birdError.textContent = "Please select a bird or choose N/A.";
            isValid = false;

            // Scroll to the bird error message
            birdError.scrollIntoView({ behavior: 'smooth', block: 'center' });
        } else if (location === '') {
            // If location is left blank (not even N/A), show an error
            event.preventDefault();
            locationError.textContent = "Please select a location or choose N/A.";
            isValid = false;

            // Scroll to the location error message
            locationError.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }

        // If the form is valid, it will submit. If not, it will stop and show the error.
        return isValid;
    });

    // Add event listener for location dropdown change
    const locationDropdown = document.getElementById('PnwLocation');
    if (locationDropdown) {
        locationDropdown.addEventListener('change', function () {
            // Get the selected option from the dropdown
            const selectedOption = this.options[this.selectedIndex];

            // Get the latitude and longitude from the selected option
            const latLong = selectedOption.getAttribute('data-latlong');

            // If latitude and longitude are available, update the hidden fields
            if (latLong) {
                const [latitude, longitude] = latLong.split(','); // Split the string into two parts
                document.getElementById('LatitudeInput').value = latitude; // Update latitude field
                document.getElementById('LongitudeInput').value = longitude; // Update longitude field
            } else {
                // If no latitude and longitude are available, clear the hidden fields
                document.getElementById('LatitudeInput').value = '';
                document.getElementById('LongitudeInput').value = '';
            }
        });
    }

    // Bird search functionality
    const birdSearchInput = document.getElementById('birdSearch');
    const birdResultsContainer = document.getElementById('birdResults');

    if (birdSearchInput && birdResultsContainer) {
        birdSearchInput.addEventListener('input', function () {
            const searchTerm = this.value.trim();
            console.log(`Search term: ${searchTerm}`); // Debugging: Log the search term

            if (searchTerm.toLowerCase() === "n/a") {
                // Clear previous results
                birdResultsContainer.innerHTML = '';

                // Create a special "N/A" option
                const naOption = document.createElement('div');
                naOption.className = 'bird-result';
                naOption.innerHTML = `<strong>N/A</strong>`;
                naOption.addEventListener('click', function () {
                    birdSearchInput.value = "N/A"; // Set the input value to "N/A"
                    document.getElementById('BirdId').value = ""; // Set the hidden BirdId field to "N/A"
                    birdResultsContainer.style.display = 'none'; // Hide the results dropdown
                });
                birdResultsContainer.appendChild(naOption);
                birdResultsContainer.style.display = 'block'; // Show the results container
            } else if (searchTerm.length >= 2) { // Only search if the user has typed at least 2 characters
                fetch(`/birds/search?term=${encodeURIComponent(searchTerm)}`)
                    .then(response => {
                        if (!response.ok) {
                            throw new Error('Network response was not ok');
                        }
                        return response.json();
                    })
                    .then(data => {
                        console.log('Data received:', data); // Debugging: Log the data
                        // Clear previous results
                        birdResultsContainer.innerHTML = '';

                        // Display up to 10 results
                        data.slice(0, 10).forEach(bird => {
                            const resultItem = document.createElement('div');
                            resultItem.className = 'bird-result';
                            resultItem.innerHTML = `
                                <strong>${bird.text}</strong>
                                <small>${bird.scientificName}</small>
                            `;
                            resultItem.addEventListener('click', function () {
                                birdSearchInput.value = bird.text; // Set the input value to the selected bird
                                document.getElementById('BirdId').value = bird.id; // Set the hidden BirdId field
                                birdResultsContainer.style.display = 'none'; // Hide the results dropdown
                            });
                            birdResultsContainer.appendChild(resultItem);
                        });

                        // Show the results container if there are results
                        if (data.length > 0) {
                            birdResultsContainer.style.display = 'block';
                        } else {
                            birdResultsContainer.style.display = 'none';
                        }
                    })
                    .catch(error => {
                        console.error('Error fetching bird species:', error); // Debugging: Log any errors
                        birdResultsContainer.style.display = 'none';
                    });
            } else {
                // Hide the results container if the search term is too short
                birdResultsContainer.style.display = 'none';
            }
        });

        // Hide the results dropdown when clicking outside
        document.addEventListener('click', function (event) {
            if (!birdSearchInput.contains(event.target) && !birdResultsContainer.contains(event.target)) {
                birdResultsContainer.style.display = 'none';
            }
        });
    }
}

// Function to scroll to the first error message
function scrollToFirstError() {
    const firstError = document.querySelector('.text-danger');
    if (firstError) {
        firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}

// Initialize the form when the DOM is fully loaded
document.addEventListener('DOMContentLoaded', initializeBirdLogForm);
///////////////




// wwwroot/js/birdlog/birdlog.js
// Function to initialize the form validation and event listeners for the Edit view
function initializeEditBirdLogForm() {
    const form = document.getElementById('sightingForm');
    if (!form) return; // Exit if the form doesn't exist on the page

    // Add event listener for form submission
    form.addEventListener('submit', function (event) {
        const birdId = document.getElementById('BirdId').value;
        const location = document.getElementById('PnwLocation').value;
        const locationError = document.getElementById('locationError'); // Error span for location
        let isValid = true;

        locationError.textContent = ""; // Reset error message

        // Ensure at least one selection is made (Bird or Location)
        if ((birdId === '' || birdId === '0') && (location === '' || location === '0')) {
            // Allow both to be N/A (0)
            // No need to prevent submission or show an error
        } else if (location === '' && location !== '0') {
            // If location is empty and not explicitly set to N/A, show an error
            event.preventDefault();
            locationError.textContent = "Please select a location or choose N/A.";
            isValid = false;

            // Scroll to the first error message
            scrollToFirstError();
        }

        return isValid; // Allow form submission if validation passes
    });

    // Ensure location selection updates hidden fields correctly
    const locationDropdown = document.getElementById('PnwLocation');
    if (locationDropdown) {
        locationDropdown.addEventListener('change', function () {
            const selectedOption = this.options[this.selectedIndex];
            const latLong = selectedOption.getAttribute('data-latlong');

            if (latLong) {
                const [latitude, longitude] = latLong.split(',');
                document.getElementById('LatitudeInput').value = latitude;
                document.getElementById('LongitudeInput').value = longitude;
            } else {
                document.getElementById('LatitudeInput').value = '';
                document.getElementById('LongitudeInput').value = '';
            }
        });
    }

    // Initialize the dropdown and hidden fields with the current values
    const selectedLatLong = "@ViewBag.SelectedLatLong"; // Get the selected location from ViewBag
    if (selectedLatLong) {
        // Find the option that matches the selected location
        for (let i = 0; i < locationDropdown.options.length; i++) {
            if (locationDropdown.options[i].value === selectedLatLong) {
                locationDropdown.selectedIndex = i; // Set the dropdown to the selected option
                break;
            }
        }

        // Update the hidden fields with the selected location's latitude and longitude
        const selectedOption = locationDropdown.options[locationDropdown.selectedIndex];
        const latLong = selectedOption.getAttribute('data-latlong');

        if (latLong) {
            const [latitude, longitude] = latLong.split(',');
            document.getElementById('LatitudeInput').value = latitude;
            document.getElementById('LongitudeInput').value = longitude;
        }
    }
}

// Function to scroll to the first error message
function scrollToFirstError() {
    const firstError = document.querySelector('.text-danger');
    if (firstError) {
        firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}

// Initialize the Edit form when the DOM is fully loaded
document.addEventListener('DOMContentLoaded', function () {
    initializeEditBirdLogForm();
});



///////////////////////////

//SD-27
//javascript for birdlog index.cshtml
//for filter and sorting
document.addEventListener("DOMContentLoaded", function () {
    const overlay = document.getElementById("filterOverlay");
    const openFilterFormButton = document.getElementById("openFilterForm");
    const closeFilterFormButton = document.getElementById("closeFilterForm");
    const birdSearch = document.getElementById("birdSearch");
    const birdDropdown = document.getElementById("birdDropdown");
    const selectedBirdsContainer = document.getElementById("selectedBirdsContainer");
    const selectedBirdsInput = document.getElementById("selectedBirds");
    const filterForm = document.getElementById("filterForm");
    const showLogsButton = document.querySelector("button[type='submit']");
    const userDropdown = document.getElementById("userId");
    const sightingsTable = document.querySelector(".table-responsive");
    const messageContainer = document.querySelector(".alert-info") || document.createElement("div");

    let selectedBirds = [];
    const allBirdNames = window.allBirdNames || [];

    // Update the dropdown with matching bird names
    function updateDropdown(searchTerm) {
        birdDropdown.innerHTML = "";
        const matchingBirds = allBirdNames.filter(bird => bird.toLowerCase().includes(searchTerm.toLowerCase()));
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

    // Update the displayed list of selected birds (chips)
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

    // Function to remove a selected bird
    window.removeBird = function (bird) {
        selectedBirds = selectedBirds.filter(b => b !== bird);
        updateSelectedBirds();
    };

    birdSearch.addEventListener("input", function () {
        updateDropdown(this.value);
    });

    openFilterFormButton.addEventListener("click", function () {
        overlay.classList.remove("d-none");
    });

    closeFilterFormButton.addEventListener("click", function () {
        overlay.classList.add("d-none");
    });

    filterForm.addEventListener("submit", function (event) {
        event.preventDefault();
        window.location.href = `/BirdLog/Index?selectedBirds=${selectedBirds.join(",")}`;
    });

    document.addEventListener("click", function (event) {
        if (!birdSearch.contains(event.target)) {
            birdDropdown.style.display = "none";
        }
    });
});
