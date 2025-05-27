// Written by Christian Raymon

// module.exports = {fetchUser, fetchSightingsByUser, makeSightingMarkers};

// Code for Map div
// Create map centered using the given default location
let map = L.map('map').setView([44.8485, -123.2340], 14);

const allSightingsGroup = L.layerGroup().addTo(map);
const usersSightingsGroup = L.layerGroup().addTo(map);

const overlayMaps ={
    "All Sightings": allSightingsGroup,
    "My Sightings": usersSightingsGroup,
};

// Add tile layer
const tiles = L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
	maxZoom: 19,
	attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);

let markers = {};

var storkDorkIcon = L.icon({
    iconUrl: '../images/map/StorkMarkerIcon.png',
    iconSize: [64, 64]
})


/* geolocation courtesy of Alice Hodge */
// document.addEventListener('DOMContentLoaded', function() {
//     setupGeolocation();
// });

function setupGeolocation() {
    if ("geolocation" in navigator) {
        navigator.geolocation.getCurrentPosition(
            function(position) {
                console.log("Geolocation success:", position.coords.latitude, position.coords.longitude);
                map.setView([position.coords.latitude, position.coords.longitude], 13);
            },
            function(error) {
                console.error("Geolocation error:", error.message);
            }
        );
    } else {
        console.warn("Geolocation not available in this browser.");
    }
}

/* --------------------------------- */

async function fetchAllSightings() {
    let url = `/api/map/GetSightings`;
    let response = await fetch(url);

    if (!response.ok)
    {
        console.error("failed to fetch sightings:", response.statusText);
        return;
    }

    let sightings = await response.json();

    allSightingsGroup.clearLayers();
    usersSightingsGroup.clearLayers();

    makeSightingMarkers(sightings);
}

async function fetchSightingsByUser(user) {
    let url = `/api/map/GetSightings/${user.id}`;
    let response = await fetch(url);

    if (!response.ok) {
        console.error("Failed to fetch sightings:", response.statusText);
        return;
    }

    let sightings = await response.json();

    allSightingsGroup.clearLayers();
    usersSightingsGroup.clearLayers();

    makeSightingMarkers(sightings, user.id);
}

async function fetchOtherSightings(userId) {
    let url = `/api/map/GetOtherSightings/${userId}`;
    let response = await fetch(url);

    if (!response.ok) {
        console.error("Failed to fetch other sightings:", response.statusText);
        return;
    }

    let sightings = await response.json();
    allSightingsGroup.clearLayers();
    makeSightingMarkers(sightings, null);
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

async function makeSightingMarkers(data, userId = null) {
    data.forEach(async sighting => {

        if (sighting.latitude && sighting.longitude) {
            const marker = L.marker([sighting.latitude, sighting.longitude], {icon: storkDorkIcon});

            allSightingsGroup.addLayer(marker);

            if (sighting.userId === userId)
                usersSightingsGroup.addLayer(marker);

            marker.addTo(map);

            // Store marker reference for later updates
            markers[sighting.sightingId] = marker;

            const popupContent = `<a href="/Bird/Details/${sighting.birdId}"><b>${sighting.commonName || 'Unknown Bird'}</b></a><br>
                                  <em>${sighting.sciName || 'Unknown'}</em><br>
                                  <strong>Spotted by: ${sighting.birder}</strong><br>
                                  ${sighting.date ? new Date(sighting.date).toLocaleDateString() : "Unknown Date"}<br>
                                  ${sighting.description || 'No notes available'}<br>
                                  <strong>Location:</strong> ${sighting.subdivision || 'Unknown'}, ${sighting.country || 'Unknown'}`;

            marker.bindPopup(popupContent);

            if (!sighting.country || !sighting.subdivision) {
                const location = await reverseGeocode(sighting.latitude, sighting.longitude);

                if (location !== "location not found") {
                    const [subdivision, country] = location.split(',');

                    // Update backend
                    await updateSightingLocation(sighting.sightingId, country.trim(), subdivision.trim());

                    // Update marker popup dynamically
                    marker.setPopupContent(`<b>${sighting.commonName || 'Unknown Bird'}</b><br>
                                            <em>${sighting.sciName || 'Unknown'}</em><br>
                                            <strong>Spotted by: ${sighting.birder}</strong><br>
                                            ${sighting.date ? new Date(sighting.date).toLocaleDateString() : "Unknown Date"}<br>
                                            ${sighting.description || 'No notes available'}<br>
                                            <strong>Location:</strong> ${subdivision.trim()}, ${country.trim()}`);

                }
            }
        }
    });
}

async function updateSightingLocation(sightingId, country, subdivision) {
    // 1. Validate input
    if (!sightingId || typeof sightingId !== "number") return;
    if (!country || !subdivision) return;
    if (country === "Unknown" && subdivision === "Unknown") return;

    // Optionally: Prevent duplicate updates (pseudo-code, requires more logic)
    // if (markers[sightingId]?.lastCountry === country && markers[sightingId]?.lastSubdivision === subdivision) return;

    const payload = {
        sightingId: sightingId,
        country: country,
        subdivision: subdivision
    };

    try {
        const response = await fetch("/api/sighting/UpdateLocation", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            // Optionally log or handle error
            return;
        }

        // Optionally: Store last updated values to prevent redundant updates
        // markers[sightingId].lastCountry = country;
        // markers[sightingId].lastSubdivision = subdivision;

    } catch (error) {
        // Optionally log error
    }
}

async function checkUserLoggedIn() {
    try {
        const response = await fetch("/api/User/is-user-logged-in");

        if (!response.ok) {
            console.warn("Login status check failed:", response.statusText);
            return false;
        }

        return await response.json();
    } catch (error) {
        console.error("Login check failed:", error);
        return false;
    }
}

/**
 * Creates a Leaflet control for linking to Nearby Sightings with the current map location
 * @returns {L.Control} The configured Nearby Sightings control
 */
function createNearbySightingsControl() {
    L.Control.NearbySightings = L.Control.extend({
        onAdd: function(map) {
            // Create the main container
            const container = L.DomUtil.create('div', 'leaflet-bar leaflet-control nearby-sightings-control');
            
            // Apply styles to container
            styleControlContainer(container);
            
            // Create and configure the link element
            const link = createControlLink(container);
            
            // Set up link behavior with map
            initializeNearbySightingsLink(link, map);
            
            // Prevent map clicks from propagating through the control
            L.DomEvent.disableClickPropagation(container);
            
            return container;
        }
    });

    return new L.Control.NearbySightings({ position: 'topright' });
}

/**
 * Style the control container element
 * @param {HTMLElement} container - The control container element
 */
function styleControlContainer(container) {
    container.style.backgroundColor = 'white';
    container.style.padding = '0';
    container.style.borderRadius = '4px';
    container.style.boxShadow = '0 2px 8px rgba(0,0,0,0.3)';
    container.style.margin = '10px';
}

/**
 * Create and style the link element within the control
 * @param {HTMLElement} container - The parent container
 * @returns {HTMLElement} The created link element
 */
function createControlLink(container) {
    const link = L.DomUtil.create('a', 'btn btn-primary', container);
    link.id = 'nearby-sightings-link';
    link.style.whiteSpace = 'nowrap';
    link.style.padding = '10px 16px';
    link.style.fontWeight = '500';
    link.style.width = '100%';
    link.style.display = 'flex';
    link.style.alignItems = 'center';
    link.innerHTML = '<i class="bi bi-binoculars-fill"></i>⠀Nearby Bird Sightings';
    return link;
}

/**
 * Initialize the nearby sightings link with map location passing
 * @param {HTMLElement} link - The link element
 * @param {L.Map} map - The Leaflet map
 */
function initializeNearbySightingsLink(link, map) {
    // Update href with current coordinates
    function updateLink() {
        const center = map.getCenter();
        link.href = `/NearbySightings/FromLocation?lat=${center.lat.toFixed(6)}&lng=${center.lng.toFixed(6)}`;
    }
    
    // Update link when map moves
    map.on('moveend', updateLink);
    
    // Initialize link on creation
    updateLink();
}

/**
 * Load user sightings based on login status
 */
async function loadUserSightings() {
    const isLoggedIn = await checkUserLoggedIn();

    if (isLoggedIn) {
        const user = await fetchUser();
        if (user && typeof user.id === "number") {
            await fetchSightingsByUser(user);
            await fetchOtherSightings(user.id);
        } else {
            console.warn("User check passed but couldn't fetch full user info.");
            await fetchAllSightings();
        }
    } else {
        await fetchAllSightings();
    }
}

map.whenReady(() => {
    setupGeolocation();
})

window.onload = async function() {
    
    await loadUserSightings();
    
    // Add the layer control
    L.control.layers(null, overlayMaps).addTo(map);
    
    // Add the nearby sightings control
    const nearbySightingsControl = createNearbySightingsControl();
    nearbySightingsControl.addTo(map);
}