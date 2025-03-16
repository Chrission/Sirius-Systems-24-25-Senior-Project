// Code for Map div
// Create map centered using the given default location
const map = L.map('map').setView([44.8485, -123.2340], 14);

// Add tile layer
const tiles = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
	maxZoom: 19,
	attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);

// const geojson = L.geoJson(globalGeoJson).addTp(map);

async function fetchSightingsByUser() {
    let user = await fetchUser();

    if (!user || typeof user.id !== "number") {
        console.error("Invalid or missing ID: ", user);
        return;
    }

    let url = `/api/map/GetSightings/${user.id}`;
    let response = await fetch(url);

    if (!response.ok) {
        console.error("Failed to fetch sightings:", response.statusText);
        return;
    }

    let sightings = await response.json();
    console.log("Fetched sightings: ", sightings);

    map.eachLayer((layer) => {
        if(layer instanceof L.Marker){
            map.removeLayer(layer);
        }
    });

    makeSightingMarkers(sightings);
}

async function fetchUser() {
    let url = "/api/User/current-user";
    try {
        let response = await fetch(url);
        
        if (!response.ok) {
            let errorText = await response.text(); // Get error message
            throw new Error(`HTTP ${response.status}: ${errorText}`);
        }

        return await response.json();
    } catch (error) {
        console.error("Error fetching user:", error);
        return null;
    }
}

async function reverseGeocode(lat, lng) {
    const url = `https://nominatim.openstreetmap.org/reverse?lat=${lat}&lon=${lng}&format=json`;

    try {
        const response = await fetch(url);
        const data = await response.json();

        if (data && data.address) {
            const country = data.address.country || "Unknown Country";
            const subdivision = data.address.state || data.address.province || "Unknown Region";

            return `${subdivision}, ${country}`;
        }
    } catch (error) {
        console.error("Reverse geocoding failed:", error);
    }
    return "Location not found";
}

function makeSightingMarkers(data)
{
    data.forEach(async sighting => {
                console.log("Sighting Data:", sighting); // Debugging

                if (sighting.latitude && sighting.longitude) { // Ensure coordinates exist

                    const locationName = await reverseGeocode(sighting.latitude, sighting.longitude);

                    L.marker([sighting.latitude, sighting.longitude])
                        .addTo(map)
                        .bindPopup(`<b>${sighting.commonName || 'Unknown Bird'}</b><br>
                                    <em>${sighting.sciName || 'Unknown'}</em><br>
                                    ${sighting.date ? new Date(sighting.date).toLocaleDateString() : "Unknown Date"}<br>
                                    ${sighting.description || 'No notes available'}<br>
                                    <strong>Location:</strong> ${locationName}`);
                }
            });
}

window.onload = function() {
    fetchSightingsByUser();
}