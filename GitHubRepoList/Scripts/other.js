/* This file contains additional js scripts */

// Toggles ui-sortable-selected (for ui.sortable.multiselection) class on table row depending on that row's checkbox state

function selectTableRow(event) {
	if (event.target.checked) {
		event.target.parentElement.parentElement.classList.add('ui-sortable-selected');
	} else {
		event.target.parentElement.parentElement.classList.remove('ui-sortable-selected');
	}
}

// Toggles ui-sortable-selected (for ui.sortable.multiselection) class for all table rows

function selectAllTableRows(event) {
	var checkboxes = Array.from(document.getElementsByClassName('row-selection-checkbox'));
	if (event.target.checked) {
		checkboxes.forEach(function(element) {
			element.checked = true;
			if (element.id != 'select-all') {
				element.parentElement.parentElement.classList.add('ui-sortable-selected');
			}
		});
	} else {
		checkboxes.forEach(function(element) {
			element.checked = false;
			element.parentElement.parentElement.classList.remove('ui-sortable-selected');
		});
	}
	
}

// Toggles loading animation near the action buttons

function toggleSpinner() {
    var spinner = document.querySelector("#loading-spinner");
    if (spinner.classList.contains('hidden')) {
        spinner.classList.remove('hidden');
    } else {
        spinner.classList.add('hidden');
    }
}

// Animates (fade-in-out) "Changes saved" message

function showChangesSavedMessage() {
    var message = document.querySelector("#success-changes-message");
    message.classList.remove("show-success-changes-message");
    setTimeout(function () {
        message.classList.add("show-success-changes-message");
    }, 10);
    setTimeout(function () {
        message.classList.remove("show-success-changes-message");
    }, 5000);
}
