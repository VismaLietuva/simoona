var progressBarMocks = {};

beforeEach(function() {
    progressBarMocks = {
        defaultDirective: '<ace-progress-bar value="10" max="10"></ace-progress-bar>',
        fullTrueDirective: '<ace-progress-bar value="10" max="10" full-danger="true"></ace-progress-bar>',
        fullUndefinedDirective: '<ace-progress-bar value="10" max="10"></ace-progress-bar>',
        halfUndefinedDirective: '<ace-progress-bar value="5" max="10"></ace-progress-bar>',
        halfTrueDirective: '<ace-progress-bar value="5" max="10" full-danger="true"></ace-progress-bar>'
    };
});
