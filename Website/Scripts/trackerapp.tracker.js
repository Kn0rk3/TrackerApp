function SignOnCtrl($scope) {
    $scope.root = root;

    // Initialize
    if ($('#Url').val().length > 0 && $('#Initials').val().length > 0) {
        $('#Password').focus();
    } else {
        $('#Url').focus();
    }
}

function DashboardCtrl($scope) {
    // ¤¤¤ Property definitions ¤¤¤
    $scope.tasks = [];
    $scope.registrations = [];
    $scope.query = '';
    $scope.selectedIndex = 1;
    $scope.selectedDate = new Date();
    $scope.registrationsTotal = 0.0;
    $scope.selectedTask = '';
    $scope.registrationHours = 0;
    $scope.registrationText = '';
    $scope.filteredTasks = [];

    // ¤¤¤ Method definitions ¤¤¤
    $scope.previousDate = function () { // Step the date one day back
        $scope.selectedDate.setDate($scope.selectedDate.getDate() - 1);
        $scope.getRegistrations();
    };

    $scope.nextDate = function () { // Step the date one day forward
        $scope.selectedDate.setDate($scope.selectedDate.getDate() + 1);
        $scope.getRegistrations();
    };

    $scope.getTasks = function () {

        // Fetch the list of tasks from the web service
        $.getJSON(root + 'Task/Get', {}, function (data) {
            $scope.$apply(function (scope) {
                // Apply the result to the scope and trigger the filter change
                scope.tasks = data.Data;
                scope.filterChange();
            });

        });

    };

    $scope.getRegistrations = function () {

        // Fetch the list of registrations from the web service
        $.getJSON(root + 'Registration/Get', { start: dateToYMD($scope.selectedDate) }, function (data) {
            $scope.$apply(function (scope) {
                // Apply the result to the scope
                scope.registrations = data.Data;

                // Fallback text if no registrations
                if (scope.registrations.length == 0) {
                    scope.registrations = [{ 'ProjectName': 'No registrations', 'Hours': 0 }];
                }

                // Calculate the total hours
                scope.registrationsTotal = 0;
                for (dat in scope.registrations) {
                    scope.registrationsTotal = scope.registrationsTotal + scope.registrations[dat].Hours;
                }
            });
        });
    };

    $scope.filterChange = function () {
        // When the filter changes, reset the selection to the first result
        $scope.selectedIndex = 2;
        setTimeout(function () {
            // Wait sligthly before updating the selection
            $scope.updateSelection();
        }, 100);
    }

    $scope.updateSelection = function() {
        // Reset the selection class from all results
        $('#taskList li').removeClass('active');

        // Find the active note and highlight
        var activeNode = $('#taskList li:nth-child(' + $scope.selectedIndex + ')');
        activeNode.addClass('active');

        // Reposition the window scroll position
        if (activeNode.position() != undefined) {
            $(document).scrollTop(activeNode.position().top - 90);
        }

        // Hide the registration band to make sure it is not visible
        $('#registrationBand').hide();
        $('#query').focus();
    };

    $scope.insertRegistration = function() {
        $('#registrationBand').hide();
        $('#query').focus();

        // Insert registration
        var activeNode = $scope.filteredTasks[$scope.selectedIndex - 2];

        $.getJSON(root + 'Registration/Insert', { date: dateToYMD($scope.selectedDate), hours: $scope.registrationHours.replace(',', '.'), message: $scope.registrationText, taskId: activeNode.Id }, function(data) {
            $scope.getRegistrations();
        });
    };

    $scope.selectTask = function () {
        
        $('#registrationBand').show();
        $scope.registrationHours = 0;
        $scope.registrationText = '';

        $('#registrationHours').focus();
        $('#registrationHours').select();
        
        // Reposition the window scroll position
        $(document).scrollTop($('#registrationBand').position().top - 10);
    };

    $scope.selectTaskClick = function(evt) {

        var tdIndex = $(evt.target).closest('li').prevAll().length + 1;
        var activeNode = $('#taskList li:nth-child(' + tdIndex + ')');
        var lastChild = activeNode.children().first().children('.task');
        
        $scope.selectedIndex = tdIndex;
        $scope.updateSelection();
        $scope.selectedTask = lastChild.html();

        $scope.selectTask();
    };
    
    $scope.selectRegistrationClick = function(evt) {
        
        var id = $(evt.target).attr('data-id');
        
        $.getJSON(root + 'Registration/Delete', { registrationId: id }, function (data) {
            $scope.getRegistrations();
        });

    };

    // Initialize
    $('#registrationBand').hide();
    $scope.getTasks();
    $scope.getRegistrations();
    $('#query').focus();

    $(document).bind('keydown', function (key) {

        var itemCount = $('#taskList li').length;
        if (key.keyCode === 40 && $scope.selectedIndex < itemCount) {
            // ARROW UP
            $scope.selectedIndex = $scope.selectedIndex + 1; $scope.updateSelection();
            $scope.registrationHours = 0;
            $scope.registrationText = '';
        }
        else if (key.keyCode === 38 && $scope.selectedIndex > 2) {
            // ARROW DOWN
            $scope.selectedIndex = $scope.selectedIndex - 1; $scope.updateSelection();
            $scope.registrationHours = 0;
            $scope.registrationText = '';
        }
        else if (key.keyCode === 13) {
            // ENTER
            if ($('#registrationBand').is(':visible')) {

                if ($('#registrationHours').is(':focus')) {
                    $('#registrationText').focus();
                    $('#registrationText').select();
                } else if ($('#registrationText').is(':focus')) {
                    $scope.insertRegistration();
                }

            } else {
                
                $scope.$apply(function (scope) {
                    var activeNode = $('#taskList li:nth-child(' + $scope.selectedIndex + ')');
                    var lastChild = activeNode.children().first().children('.task');
                    scope.selectedTask = lastChild.html();
                    scope.selectTask();
                });
                
            }
        }
        else if (key.keyCode == 27) {
            // ESC
            $('#registrationBand').hide();
            $('#query').focus();
        }
        
    });

}

function dateToYMD(date) {
    var d = date.getDate();
    var m = date.getMonth() + 1;
    var y = date.getFullYear();
    return '' + y + '-' + (m <= 9 ? '0' + m : m) + '-' + (d <= 9 ? '0' + d : d);
}