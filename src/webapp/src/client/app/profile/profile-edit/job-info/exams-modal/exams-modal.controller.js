(function () {

    'use strict';

    angular
        .module('simoonaApp.Profile')
        .controller('examsModalCtrl', examsModalCtrl);

    examsModalCtrl.$inject = ['$scope', '$filter', '$uibModalInstance', 'profileRepository', '$translate', 'certificate'];

    function examsModalCtrl($scope, $filter, $uibModalInstance, profileRepository,$translate, certificate) {

        
        $scope.allCertificates = allCertificates;
        $scope.allExams = allExams;
        $scope.selectCertificate = selectCertificate;
        $scope.selectExam = selectExam;
        $scope.addEmptyExam = addEmptyExam;
        $scope.deleteExam = deleteExam;

        $scope.save = save;
        $scope.cancel = cancel;
        $scope.passExam = passExam;

        
        init();

        function init() {
            activate();

            function activate() {
                if (certificate) {
                    $scope.certificate = angular.copy(certificate);
                }
                else {
                    $scope.certificate = {
                        name: '',
                        inProgress: false,
                        exams: []
                    };
                }

                if ($scope.certificate.exams && $scope.certificate.exams.length == 0) {
                    addEmptyExam();
                }
            }
        }
      
        function allCertificates(search) {
            return profileRepository.getCertificateForAutoComplete({ s: search });
        }

        function allExams(exam, search) {
            return profileRepository.GetExamForAutoCompleteByTitleAndNumber({ title: exam.title, number: exam.number });
        }

        function selectCertificate(model) {
            $scope.certificate = model;
        }

        function addEmptyExam() {
            $scope.certificate.exams.push({ title: '', number: '' });
        }

        function selectExam(exam, model) {
            exam.title = model.title;
            exam.number = model.number;
        }

        function deleteExam(exam) {
            var index = $scope.certificate.exams.indexOf(exam);
            $scope.certificate.exams.splice(index, 1);
        }

        function save() {
            clearEmptyExams();
            $uibModalInstance.close($scope.certificate);
        }

        function cancel() {
            $uibModalInstance.dismiss('cancel');
        }

        function passExam() {
            $scope.certificate.inProgress = !!$scope.certificate.inProgress;
        }

        function clearEmptyExams() {
            for (var i = 0; i < $scope.certificate.exams.length; i++) {
                if (!$scope.certificate.exams[i].title) {
                    $scope.certificate.exams.splice(i, 1);
                    i--;
                }
            }
        }
    }
})();