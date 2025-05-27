function initializeEditBirdLogForm() {
    // Initialize shared functionality from create.js
    initializeBirdLogForm();

    // EDIT-SPECIFIC FUNCTIONALITY
    // 0. Initialize existing bird information
    const initialBirdId = document.getElementById('initialBirdId').value;
    const initialBirdName = document.getElementById('initialBirdName').value;

    if (initialBirdId && initialBirdName) {
        document.getElementById('birdSearch').value = initialBirdName;
        document.getElementById('commonNameDisplay').textContent = initialBirdName;
        document.getElementById('scientificNameDisplay').textContent = 
            document.getElementById('scientificNameDisplay').dataset.initialScientific || '-';
    }

    // 1. Photo preview initialization
    const existingPhotoUrl = document.getElementById('photoPreview').src;
    const photoPreviewContainer = document.getElementById('photoPreviewContainer');
    if (existingPhotoUrl && !existingPhotoUrl.includes('undefined')) {
        photoPreviewContainer.style.display = 'block';
        addRemovePhotoButton();
    }

    function addRemovePhotoButton() {
        const uploadContainer = document.querySelector('.upload-placeholder');
        if (!document.getElementById('removePhotoBtn')) {
            const removeBtn = document.createElement('button');
            removeBtn.type = 'button';
            removeBtn.id = 'removePhotoBtn';
            removeBtn.className = 'btn btn-sm btn-danger mt-2';
            removeBtn.innerHTML = '<i class="bi bi-trash"></i> Remove Photo';
            removeBtn.addEventListener('click', function() {
                photoPreviewContainer.style.display = 'none';
                document.getElementById('photoFile').value = '';
                document.getElementById('photoPreview').src = '';
                const hiddenInput = document.createElement('input');
                hiddenInput.type = 'hidden';
                hiddenInput.name = 'removePhoto';
                hiddenInput.value = 'true';
                document.getElementById('sightingForm').appendChild(hiddenInput);
                this.remove();
            });
            uploadContainer.appendChild(removeBtn);
        }
    }

    // 2. Map initialization with existing coordinates
    const latInput = document.getElementById('LatitudeInput');
    const lngInput = document.getElementById('LongitudeInput');
    if (latInput && lngInput) {
        const lat = parseFloat(latInput.value);
        const lng = parseFloat(lngInput.value);
        
        if (!isNaN(lat) && !isNaN(lng)) {
            setTimeout(() => {
                if (window.createMap) {
                    window.createMap.setView([lat, lng], 14);
                    updateLocation(L.latLng(lat, lng));
                }
            }, 300);
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    if (typeof L !== 'undefined') {
        initializeEditBirdLogForm();
    }
});