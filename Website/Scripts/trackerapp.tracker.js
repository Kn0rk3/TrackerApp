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
    $scope.tasks = [];
    $scope.registrations = [];
    $scope.query = '';
    $scope.selectedIndex = 1;
    $scope.selectedDate = new Date();
    $scope.registrationsTotal = 0.0;
    $scope.selectedTask = '';
    $scope.registrationHours = 0;
    $scope.registrationText = '';

    $scope.previousDate = function () {
        $scope.selectedDate.setDate($scope.selectedDate.getDate() - 1);
        $scope.getRegistrations();
    };

    $scope.nextDate = function () {
        $scope.selectedDate.setDate($scope.selectedDate.getDate() + 1);
        $scope.getRegistrations();
    };

    $scope.getTasks = function () {

        $.getJSON(root + 'Task/Get', {}, function (data) {
            $scope.$apply(function (scope) {
                scope.tasks = data.Data;
                scope.filterChange();
            });

        });

    };

    $scope.getRegistrations = function () {

        $.getJSON(root + 'Registration/Get', { start: dateToYMD($scope.selectedDate) }, function (data) {
            $scope.$apply(function (scope) {
                scope.registrations = data.Data;

                if (scope.registrations.length == 0) {
                    scope.registrations = [{ 'ProjectName': 'No registrations', 'Hours': 0 }];
                }

                scope.registrationsTotal = 0;
                for (dat in scope.registrations) {
                    scope.registrationsTotal = scope.registrationsTotal + scope.registrations[dat].Hours;
                }
            });

        });

    };

    $scope.filterChange = function () {
        $scope.selectedIndex = 2;
        setTimeout(function () {
            $scope.updateSelection();
        }, 100);
    }

    $scope.updateSelection = function () {
        $('#tasks tr').removeClass('active');
        $('#tasks tr td:last-child').html('');
        var activeNode = $('#tasks tr:nth-child(' + $scope.selectedIndex + ')');
        activeNode.addClass('active');

        if (activeNode.position() != undefined) {
            $(document).scrollTop(activeNode.position().top - 50);
        }

        $('#registrationBand').hide();
        $('#query').focus();
    }

    // Initialize
    $('#registrationBand').hide();
    $scope.getTasks();
    $scope.getRegistrations();
    $('#query').focus();

    $(document).bind('keydown', function (key) {

        var itemCount = $('#tasks tr').length;
        if (key.keyCode === 40 && $scope.selectedIndex < itemCount) {
            // ARROW UP
            $scope.selectedIndex = $scope.selectedIndex + 1; $scope.updateSelection();
        }
        else if (key.keyCode === 38 && $scope.selectedIndex > 2) {
            // ARROW DOWN
            $scope.selectedIndex = $scope.selectedIndex - 1; $scope.updateSelection();
        }
        else if (key.keyCode === 13) {
            // ENTER
            if ($('#registrationBand').is(':visible')) {

                if ($('#registrationHours').is(':focus')) {
                    $('#registrationText').focus();
                    $('#registrationText').select();
                } else if ($('#registrationText').is(':focus')) {
                    $('#registrationBand').hide();
                    $('#query').focus();

                    // Insert registration
                    var activeNode = $scope.tasks[$scope.selectedIndex - 2];
                    console.log(activeNode.Name);

                    $.getJSON(root + 'Registration/Insert', { date: dateToYMD($scope.selectedDate), hours: $scope.registrationHours, message: $scope.registrationText, taskId: activeNode.Id }, function (data) {
                        $scope.getRegistrations();
                    });
                }

            } else {

                $scope.$apply(function (scope) {
                    var activeNode = $('#tasks tr:nth-child(' + $scope.selectedIndex + ')');
                    var lastChild = activeNode.children('td:nth-child(3)').children('span');
                    scope.selectedTask = lastChild.html();
                });

                $('#registrationBand').show();
                $('#registrationHours').focus();
                $('#registrationHours').select();
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