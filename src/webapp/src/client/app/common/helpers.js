if (!String.format) {
    String.format = function (format) {
        var args = Array.prototype.slice.call(arguments, 1);
        if (format) {
            return format.replace(/{(\d+)}/g, function (match, number) {
                return typeof args[number] !== 'undefined' ? args[number] : match;
            });
        } else {
            return 'undefined string';
        }

    };
}

if (!String.prototype.contains) {
    String.prototype.contains = function () {
        return String.prototype.indexOf.apply(this, arguments) !== -1;
    };
}

if (!Array.prototype.contains) {
    Array.prototype.contains = function (item) {
        for (var i = 0; i < this.length; i++) {
            if (this[i] === item) {
                return true;
            }
        }
        return false;
    };
}

if (!String.prototype.toLowerCaseFirstChar) {
    String.prototype.toLowerCaseFirstChar = function () {
        return this.substr(0, 1).toLowerCase() + this.substr(1);
    };
}

Raphael.st.contains = function (item) {
    var contains = false;
    this.forEach(function (el) {
        if (el.id === item.id) {
            contains = true;
        }
    });
    return contains;
};

Math.toPrecise = function (number) {
    return +(number).toPrecision(15);
};

if (!Array.prototype.remove) {
    Array.prototype.remove = function () {
        var args = arguments[0].constructor === Array ? arguments[0] : Array.prototype.slice.call(arguments);

        var indices = this
            .map(function (a, i) {
                if (~args.indexOf(a)) return i;
            })
            .filter(function (a) {
                return a === 0 ? true : a;
            });

        for (var i = indices.length; i > 0; i--) {
            this.splice(indices[i - 1], 1);
        }

        return this;
    };
}