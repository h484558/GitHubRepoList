angular.module('githubRepoList', ['ngDialog', 'ui.sortable', 'ui.sortable.multiselection', 'ngCookies'])
    .service('authCookieService', function ($cookies) {
        this.isAdmin = function () {
            return $cookies.getObject("login_info") == undefined ? false : $cookies.getObject("login_info").is_admin;
        };

        this.isWrite = function () {
            return $cookies.getObject("login_info") == undefined ? false : $cookies.getObject("login_info").is_write;
        };

        this.isRead = function () {
            return $cookies.getObject("login_info") == undefined ? false : $cookies.getObject("login_info").is_read;
        };

        this.getLogin = function () {
            return $cookies.getObject("login_info") == undefined ? null : $cookies.getObject("login_info").login;
        }
    })
	.controller('AppController', function($scope, $http, $filter, $cookies, $timeout, ngDialog, uiSortableMultiSelectionMethods, authCookieService) {
		angular.element(document).ready(function() {
			document.body.addEventListener('click', function(e) {
				if (e.target != $scope.repoObjFieldInput && $scope.repoObjFieldInput != undefined) {
					$scope.repoObjFieldInput.blur();
				}
			});
		});

        // USE USERFACTORY FOR USER TABLE
		
		// Doubleclick action switch
		$scope.mode = 'details';
		
		$scope.switchMode = function() {
			if ($scope.mode == 'details') {
				$scope.mode = 'edit';
				document.querySelector("#display-mode-details").classList.remove('btn-primary');
				document.querySelector("#display-mode-edit").classList.add('btn-primary');
			} else {
				$scope.mode = 'details';
				document.querySelector("#display-mode-edit").classList.remove('btn-primary');
				document.querySelector("#display-mode-details").classList.add('btn-primary');
			}				
		};

		$scope.authorisation = {};

	    // Creates/clears authorisation object

		$scope.refreshAuthorisation = function () {
		    $scope.authorisation = {
		        is_admin: authCookieService.isAdmin(),
		        is_write: authCookieService.isWrite(),
                is_read: authCookieService.isRead(),
		        login: authCookieService.getLogin()
		    };
		};

		$scope.refreshAuthorisation();

		$scope.signInDialogStatus = authCookieService.getLogin() == null ? true : false;

	    // Initialize table

		$scope.initTable = function () {
		    if (authCookieService.getLogin() != null && !authCookieService.isRead()) {
		        if (!authCookieService.isAdmin()) {
		            return;
		        }
		    }

		    toggleSpinner();
		    $scope.githubRepos = {};
		    $http.get("/Repo")
			.then(function (response) {
			    $scope.githubRepos = response.data;
			    $scope.newRepoObject = angular.copy($scope.githubRepos[0]);
			    toggleSpinner();
			},
            function (response) {
                $scope.openSignInDialog();
                toggleSpinner();
            });
		};

		$scope.initTable();
		
	    // Save updated sort_positions on server

		$scope.updateSortingOrderOnServer = function () {
		    if (!authCookieService.isWrite()) {
		        if (!authCookieService.isAdmin()) {
		            return;
		        }
		    }

	        var sortedGithubRepos = [];

	        $scope.githubRepos.forEach(function (repo, i) {
	            repo.sort_position = i;
	            sortedGithubRepos.push(repo);
	        });

	        toggleSpinner();

	        $http.post("/Repo/EditSortPositions", { repos: sortedGithubRepos }).then(function (response) {
	            toggleSpinner();

	            if (response.data.status == 'OK') {
	                showChangesSavedMessage();
	            } else {
	                ngDialog.open({ template: "<p>Failed updating repo positions</p>", plain: true });
	            }
	        },
            function (response) {
                ngDialog.open({ template: "<p>" + response.data.message + "</p>", plain: true });
            });
	    };
		
        // Sorts table by fieldName

	    $scope.sortBy = function (fieldName) {
			$scope.reverse = (fieldName == $scope.fieldName) ? !$scope.reverse : false;
			$scope.fieldName = fieldName;
			$scope.githubRepos = $filter('orderBy')($scope.githubRepos, $scope.fieldName, $scope.reverse);

			if (!authCookieService.isWrite()) {
			    if (!authCookieService.isAdmin()) {
			        return;
			    }
			}

			$scope.updateSortingOrderOnServer();
		};

	    // Sortable options required by ui.sortable.multiselection
		
	    $scope.sortableOptions = uiSortableMultiSelectionMethods.extendOptions({
			stop: function(e, ui) {     // Uncheck all table checkboxes (1st td) when sorting has been completed
				Array.from(document.getElementsByClassName('row-selection-checkbox')).forEach(function(element) {
				    element.checked = false;
				});

				if (!authCookieService.isWrite()) {
				    if (!authCookieService.isAdmin()) {
				        return;
				    }
				}

				$scope.updateSortingOrderOnServer();    // Save updated sort_positions on server
			}
		});
		
		$scope.repoObjFieldInput;
		
	    // Replaces span with input

		$scope.toggleEdit = function ($event) {
		    if (!authCookieService.isWrite()) {
		        if (!authCookieService.isAdmin()) {
		            return;
		        }
		    }

			var repoObjFieldSpan = $event.currentTarget.querySelector('.field-value') || $event.currentTarget.parentElement.querySelector('.field-value');
			$scope.repoObjFieldInput = $event.currentTarget.querySelector('.field-edit') || $event.currentTarget.parentElement.querySelector('.field-edit');
			
			if (repoObjFieldSpan.classList.contains('hidden')) {
				repoObjFieldSpan.classList.remove('hidden');
			} else {
				repoObjFieldSpan.classList.add('hidden');
			}
			
			if ($scope.repoObjFieldInput.classList.contains('hidden')) {
				$scope.repoObjFieldInput.classList.remove('hidden');
				$scope.repoObjFieldInput.focus();
			} else {
				$scope.repoObjFieldInput.classList.add('hidden');
			}
		};

        // ng-dblclick replaces span with input for selected td element using toggleEdit function
		
		$scope.editField = function($event) {
			if ($scope.mode != 'edit') {
				return;
			}
			
			$scope.toggleEdit($event);
		};

        // Saves changes made by user
		
		$scope.saveChanges = function ($event) {
		    if (!authCookieService.isWrite()) {
		        if (!authCookieService.isAdmin()) {
		            return;
		        }
		    }

		    var repoToEditID = $event.target.parentElement.parentElement.getElementsByTagName("td")[1].children[0].innerHTML;   // Get repo ID

		    var repoToEdit = $scope.githubRepos.filter(function (repo) {    // Find repo object with repoToEditID
			    return repo.id == repoToEditID;
			});

		    repoToEdit = repoToEdit[0];

		    toggleSpinner();

		    $http.post("/Repo/Edit/" + repoToEdit.id, { repo: repoToEdit }).then(function (response) {
		        toggleSpinner();

		        if (response.data.status == 'OK') {
		            $http.get("/Repo")
                        .then(function (repoUpdateResponse) {
                            $scope.githubRepos = repoUpdateResponse.data;
                        });
		            showChangesSavedMessage();
		        } else {
		            ngDialog.open({ template: '<p>An error occured while deleting selected repo(s)!</p>', plain: true });
		        }
		    },
            function (response) {
                ngDialog.open({ template: "<p>" + response.data.message + "</p>", plain: true });
            });

			$scope.toggleEdit($event);
		};

	    // Shows repo details in dialog window based on repo-details-tpl template
		
		$scope.showRepoDetails = function (repo) {
		    if (!authCookieService.isRead()) {
		        if (!authCookieService.isAdmin()) {
		            return;
		        }
		    }

			if ($scope.mode != 'details') {
				return;
			}
			
			$scope.repo = repo;
			ngDialog.open({ template: 'repo-details-tpl', scope: $scope, width: '70%' });
		};
		
	    // Creates empty repo object and opens dialog based on new-repo-tpl template

		$scope.addNewRepo = function () {
		    $scope.newRepoFormData = new FormData();

			$scope.repo = {
			    created_at: null,
			    description: null,
			    fork: null,
			    forks_url: null,
                full_name: null,
			    git_url: null,
			    html_url: null,
			    id: null,
			    name: null,
			    private: null,
			    sort_position: null,
			    updated_at: null,
			    url: null,
			    owner: {
			        avatar_url: null,
			        id: null,
			        login: null,
			        repos_url: null,
			        type: null,
			        url: null,
                    userpic: null
			    }
			};

			ngDialog.open({ template: 'new-repo-tpl', scope: $scope, width: '70%' });
		};

	    
        // New repo request should be sent using FormData to handle image uploading

		$scope.newRepoFormData = new FormData();

	    // Fires on userpic file input change

		$scope.onBinUserpicFileChange = function (files) {
		    $scope.newRepoFormData = new FormData();
		    $scope.newRepoFormData.append('userpicFile', files[0]);
		};

	    // Creates new repo on server
		
		$scope.saveNewRepo = function() {
		    toggleSpinner();
		    // var reader = new FileReader();
		    // var userpicFile = document.querySelector("#userpic-file-input").files[0];

		    var postRepo = function () {
		        $http.post("/Repo/Create", $scope.newRepoFormData, {
		            withCredentials: true,
		            transformRequest: angular.identity,
		            headers: { 'Content-Type': undefined }
		        }).then(function (response) {
		            toggleSpinner();
		            if (response.data.status == 'OK') {
		                // If user is unauthorised or doesn't have read privileges -> show dialog with success message
		                if (authCookieService.getLogin() == null || !authCookieService.isRead()) {
		                    if (!authCookieService.isAdmin()) {
		                        ngDialog.open({
		                            template: "<p>Your repo has been successfully created. Thank you!</p>",
		                            plain: true,
		                            width: 400,
		                            className: 'ngdialog-theme-default successful-repo-creation-dialog'
		                        });
		                    } else {
		                        $http.get("/Repo")
                                .then(function (repoUpdateResponse) {
                                    $scope.githubRepos = repoUpdateResponse.data;
                                });
		                        showChangesSavedMessage();
		                    }
		                } else {    // Else update repo list and show "Changes saved" messages
		                    $http.get("/Repo")
                            .then(function (repoUpdateResponse) {
                                $scope.githubRepos = repoUpdateResponse.data;
                            });
		                    showChangesSavedMessage();
		                }
		            } else {
		                ngDialog.open({ template: "<p>An error occured while creating new repo</p>", plain: true });
		                return false;
		            }
		        },
                function (response) {
                    toggleSpinner();
                    if (response.status == 404) {
                        ngDialog.open({ template: "<p>File is too big! Max. filesize is 10 MB</p>", plain: true, width: 400, className: 'ngdialog-theme-default error-dialog'  });
                    } else {
                        ngDialog.open({ template: "<p>" + response.data.message + "</p>", plain: true, width: 400, className: 'ngdialog-theme-default error-dialog' });
                    }
                    
                });
		    }

		    var convertRepoObjectToFormData = function () {     // Converting object to form data is essential for sending it alongside with file
		        for (var key in $scope.repo) {
		            if (isObject($scope.repo[key])) {
		                for (var subKey in $scope.repo[key]) {
		                    $scope.newRepoFormData.append(key + "[" + subKey + "]", $scope.repo[key][subKey]);
		                }
		            } else {
		                $scope.newRepoFormData.append(key, $scope.repo[key]);
		            }
		        }
		    };

		    //if (userpicFile) {
		    //    reader.readAsDataURL(userpicFile);
		    //    reader.onload = function () {
		    //        $scope.repo.owner.userpic = reader.result;
		    //        convertRepoObjectToFormData();
		    //        postRepo();
		    //    }
		    //} else {
		        convertRepoObjectToFormData();
		        postRepo();
		    //}

			return true;
		};
		
	    // Delete repos selected with checkboxes

		$scope.deleteSelectedRepos = function () {
		    if (!authCookieService.isWrite()) {
		        if (!authCookieService.isAdmin()) {
		            return;
		        }
		    }

		    var idsForRemoval = [];
		    Array.from(document.getElementsByClassName('ui-sortable-selected')).forEach(function (element) {    // Selected repos contain ui-sortable-selected class 
		        idsForRemoval.push($scope.githubRepos[element.rowIndex - 1].id);
		    });

		    toggleSpinner();

		    $http.post("/Repo/Delete", { ids: idsForRemoval }).then(function (response) {
		        toggleSpinner();

		        if (response.data.status == 'OK') {
		            $http.get("/Repo")
                        .then(function (repoUpdateResponse) {
                            $scope.githubRepos = repoUpdateResponse.data;
                        });
		            showChangesSavedMessage();
		        } else {
		            ngDialog.open({ template: '<p>An error occured while deleting selected repo(s)!</p>', plain: true });
		        }
		    },
            function (response) {
                ngDialog.open({ template: "<p>" + response.data.message + "</p>", plain: true });
            });
		};

		$scope.FormData = {
		    githubImportUsername: "google"
		};

		$scope.importFromGithubModal = function () {
		    ngDialog.open({ template: "import-from-github-modal-tpl", scope: $scope });
		};

		$scope.importReposFromGithub = function () {
		    if (!authCookieService.isWrite()) {
		        if (!authCookieService.isAdmin()) {
		            return;
		        }
		    }

		    toggleSpinner();
		    $http.post("/Repo/ImportReposFromGithub", { login: $scope.FormData.githubImportUsername }).then(function (response) {
		        toggleSpinner();
		        if (response.data.status == 'OK') {
		            $http.get("/Repo")
                        .then(function (repoUpdateResponse) {
                            $scope.githubRepos = repoUpdateResponse.data;
                        });
		            showChangesSavedMessage();
		        } else {
		            ngDialog.open({ template: '<p>An error occured while importing repos!</p>', plain: true });
		            return false;
		        }
		    },
            function (response) {
                ngDialog.open({ template: "<p>" + response.data.message + "</p>", plain: true });
            });

		    return true;
		};

		$scope.importFromJsonModal = function () {
		    ngDialog.open({ template: "import-from-json-modal-tpl", scope: $scope });
		};

		$scope.jsonImportFormData = new FormData();

	    // Fires on json file input change

		$scope.onJsonFileChange = function (files) {
		    $scope.jsonImportFormData = new FormData();
		    $scope.jsonImportFormData.append('jsonFile', files[0]);
		};

	    // Imports repos from JSON. POST options (withCredentials, headers ....) are ESSENTIAL for successful file upload

		$scope.importReposFromJson = function () {
		    if (!authCookieService.isWrite()) {
		        if (!authCookieService.isAdmin()) {
		            return;
		        }
		    }

		    toggleSpinner();
		    $http.post("/Repo/ImportReposFromJson", $scope.jsonImportFormData, {
                withCredentials: true,
		        transformRequest: angular.identity,
		        headers: { 'Content-Type': undefined }
		    }).then(function (response) {
		        toggleSpinner();
		        if (response.data.status == 'OK') {
		            $http.get("/Repo")
                        .then(function (repoUpdateResponse) {
                            $scope.githubRepos = repoUpdateResponse.data;
                        });
		            showChangesSavedMessage();
		        } else {
		            ngDialog.open({ template: '<p>An error occured while importing repos!</p>', plain: true });
		            return false;
		        }
		    },
            function (response) {
                ngDialog.open({ template: "<p>" + response.data.message + "</p>", plain: true });
            });

		    return true;
		};

	    // Sign In button action

		$scope.signInCredentials = {
		    login: null,
            password: null
		};

		$scope.signIn = function (closeDialogFunc) {
		    $http.post("/User/SignIn", $scope.signInCredentials).then(function (response) {
		        closeDialogFunc();
		        $scope.initTable();
		        //$scope.authorisation = $cookies.getObject("login_info");
		        $scope.refreshAuthorisation();
		        $scope.signInDialogStatus = false;
		        $scope.$broadcast('userSignIn', { });   // Broadcast userSignIn event to get user table data for admin user
		    },
            function (response) {
                ngDialog.open({
                    template: '<p>Failed to sign in! Please check your login credentials</p>',
                    plain: true,
                    width: 420,
                    className: 'ngdialog-theme-default sign-in-out-error-dialog'
                });
                
            });

		    return true;
		};

	    // Sign In Dialog actions

		$scope.closeSignInDialog = function () {
		    $scope.signInDialogStatus = false;
		    return true;
		};

		var signInDialog;

		$scope.openSignInDialog = function () {
		    $scope.signInDialogStatus = true;
		    signInDialog = ngDialog.open({
		        template: "login-form-tpl",
		        scope: $scope,
		        width: 400,
		        className: 'ngdialog-theme-default sign-in-dialog'
		    });

		    signInDialog.closePromise.then(function (data) {
		        $scope.closeSignInDialog();
		    });
		};

	    // Sign Out button action

		$scope.signOut = function () {
		    $scope.signInDialogStatus = true;
		    $http.post("/User/SignOut").then(function (response) {
		        $scope.initTable();     // This calls openSignInDialog if GET /Repo returns 401 but we set signInDialogStatus earlier in this function to avoid text flickering
		        $scope.refreshAuthorisation();
		    },
            function (response) {
                ngDialog.open({ template: '<p>Failed to sign out</p>', plain: true, width: 420, className: 'ngdialog-theme-default sign-in-out-error-dialog' });
            });
		};
	})
    .controller('UserlistController', function ($scope, $http, $filter, authCookieService, ngDialog) {
        $scope.userList = { };

        $scope.getUserList = function () {
            if (!authCookieService.isAdmin()) {
                return;
            }

            $http.get("/User").then(function (response) {
                $scope.userList = response.data;
            });
        };

        $scope.getUserList();

        $scope.updateUserRights = function (user, unmodifiedUser) {
            toggleSpinner();
            $http.post("/User/Edit/" + user.id, { user: user }).then(function (response) {
                if (response.data.status == 'OK') {
                    showChangesSavedMessage();
                } else {
                    if (response.data.message != null) {
                        ngDialog.open({ template: "<p>" + response.data.message + "</p>", plain: true, width: 400, className: 'ngdialog-theme-default error-dialog' });
                        for (key in user) {
                            user[key] = unmodifiedUser[key];
                        }
                    } else {
                        ngDialog.open({ template: "<p>Failed updating user rights!</p>", plain: true, width: 400, className: 'ngdialog-theme-default error-dialog' });
                    }
                }
                toggleSpinner();
            },
            function (response) {
                ngDialog.open({ template: "<p>" + response.data.message + "</p>", plain: true });
            });
        };

        $scope.$on('userSignIn', function (event, args) {
            $scope.getUserList();
        });
    })
    .directive('noAuthenticationRequired', function ($compile, authCookieService) {
        return {
            restrict: 'A',
            link: function ($scope, element, attrs) {
                $scope.$watch(function () { return authCookieService.getLogin(); }, function (value) {
                    if (authCookieService.getLogin() == null) {
                        element.removeClass("hidden");
                    } else {
                        element.addClass("hidden");
                    }
                });
            }
        };
    })
    .directive('authenticationRequired', function ($compile, authCookieService) {
        return {
            restrict: 'A',
            priority: 1,
            link: function ($scope, element, attrs) {
                $scope.$watch(function () { return authCookieService.getLogin(); }, function (value) {
                    if (authCookieService.getLogin() != null) {
                        element.removeClass("hidden");
                    } else {
                        element.addClass("hidden");
                    }
                });
            }
        };
    })
    .directive('adminRequired', function ($compile, authCookieService) {
        return {
            restrict: 'A',
            link: function ($scope, element, attrs) {
                $scope.$watch(function () { return authCookieService.isAdmin(); }, function (value) {
                    if (authCookieService.isAdmin()) {
                        element.removeClass("hidden");
                    } else {
                        element.addClass("hidden");
                    }
                });
            }
        };
    })
    .directive('writeRequired', function ($compile, authCookieService) {
        return {
            restrict: 'A',
            link: function ($scope, element, attrs) {
                $scope.$watch(function () { return authCookieService.isWrite() || authCookieService.isAdmin();; }, function (value) {
                    if (authCookieService.isWrite() || authCookieService.isAdmin()) {
                        element.removeClass("hidden");
                    } else {
                        element.addClass("hidden");
                    }
                });
            }
        };
    })
    .directive('readRequired', function ($compile, authCookieService) {
        return {
            restrict: 'A',
            link: function ($scope, element, attrs) {
                
                $scope.$watch(function () { return authCookieService.isRead() || authCookieService.isAdmin(); }, function (value) {
                    if (authCookieService.isRead() || authCookieService.isAdmin()) {
                        element.removeClass("hidden");
                    } else {
                        element.addClass("hidden");
                    }
                });
            }
        };
    })
    .directive('noReadRequired', function ($compile, authCookieService) {
        return {
            restrict: 'A',
            priority: 2,
            link: function ($scope, element, attrs) {
                $scope.$watch(function () { return authCookieService.isRead() || authCookieService.getLogin(); }, function (value) {
                    if (!authCookieService.isRead() && !authCookieService.isAdmin()) {
                        element.removeClass("hidden");
                    } else {
                        element.addClass("hidden");
                    }
                });
            }
        };
    });
