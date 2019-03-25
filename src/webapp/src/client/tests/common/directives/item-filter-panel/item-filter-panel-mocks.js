var itemFilterPanelMocks = {};

beforeEach(function() {
    itemFilterPanelMocks = {
        lodash: {},
        filters: [{
            "label": "Job title",
            "usersModel": null,
            "filter": "jobtitle",
            "placeholder": "Add job titles"
        }, {
            "label": "Projects",
            "usersModel": null,
            "filter": "projects",
            "placeholder": "Add projects"
        }, {
            "label": "Skills",
            "usersModel": null,
            "filter": "skills",
            "placeholder": "Add skills"
        }],
        filterJson: [{
            "key": "jobtitle",
            "values": [".Net Developer", "Java Developer"]
        }, {
            "key": "projects",
            "values": ["Shrooms"]
        }, {
            "key": "skills",
            "values": ["C#"]
        }],
        resource: {
            "tagInputPlaceholder": "Add a tag"
        },
        executeFilters: [{
            "label": "Job title",
            "usersModel": null,
            "filter": "jobtitle",
            "placeholder": "Add job titles",
            "model": [{
                "name": ".Net Developer"
            }, {
                "name": "Java Developer"
            }]
        }, {
            "label": "Projects",
            "usersModel": null,
            "filter": "projects",
            "placeholder": "Add projects",
            "model": [{
                "name": "Shrooms"
            }]
        }, {
            "label": "Skills",
            "usersModel": null,
            "filter": "skills",
            "placeholder": "Add skills",
            "model": [{
                "title": "C#",
                "name": "C#",
                "id": 4
            }]
        }],
        expectedResultFilterPanel: {
            "filters": [{
                "label": "Job title",
                "usersModel": null,
                "filter": "jobtitle",
                "placeholder": "Add job titles",
                "model": [".Net Developer", "Java Developer"]
            }, {
                "label": "Projects",
                "usersModel": null,
                "filter": "projects",
                "placeholder": "Add projects",
                "model": ["Shrooms"]
            }, {
                "label": "Skills",
                "usersModel": null,
                "filter": "skills",
                "placeholder": "Add skills",
                "model": ["C#"]
            }],
            "isOpen": true
        },
        expectedResultExecuteFilter: [{
            "key": "jobtitle",
            "values": [".Net Developer", "Java Developer"]
        }, {
            "key": "projects",
            "values": ["Shrooms"]
        }, {
            "key": "skills",
            "values": ["C#"]
        }]
    }
});
