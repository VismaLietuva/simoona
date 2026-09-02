(function () {
    'use strict';

    angular
        .module('simoonaApp.Events')
        .factory('eventSignUpSteps', eventSignUpSteps);

    /**
     * Sign-up question tree for the join dialog. Mirrors the rules the API
     * applies on join (EventAnswerValidator): a question shows only when the
     * option it depends on is chosen on a question that is itself showing, and a
     * required visible question must be answered.
     *
     * Answers are held as { questionId: [optionId, ...] }.
     */
    function eventSignUpSteps() {
        return {
            visibleQuestions: visibleQuestions,
            pruneAnswers: pruneAnswers,
            toggleAnswer: toggleAnswer,
            isAnswerSelected: isAnswerSelected,
            missingRequired: missingRequired,
            answerIds: answerIds,
            prefill: prefill
        };

        //////

        // Order then id, matching every read projection: a tie must not reshuffle
        // the steps between requests.
        function ordered(questions) {
            return (questions || []).slice().sort(function (a, b) {
                return a.order - b.order || a.id - b.id;
            });
        }

        function hasCondition(question) {
            return question.showIfOptionId !== null &&
                question.showIfOptionId !== undefined;
        }

        /**
         * One forward pass is enough: the API guarantees a condition points at an
         * option owned by a question of strictly lower order, and options are
         * banked only once their own question is known to be showing — so a
         * stranded answer on a hidden question can never re-open its children.
         */
        function visibleQuestions(questions, answers) {
            var chosen = {};
            var visible = [];

            ordered(questions).forEach(function (question) {
                if (hasCondition(question) && !chosen[question.showIfOptionId]) {
                    return;
                }

                visible.push(question);

                (answers[question.id] || []).forEach(function (optionId) {
                    chosen[optionId] = true;
                });
            });

            return visible;
        }

        // Drops answers to questions that are no longer showing, so the summary
        // and the submitted payload can never disagree with what is on screen.
        function pruneAnswers(questions, answers) {
            var kept = {};

            visibleQuestions(questions, answers).forEach(function (question) {
                var picked = answers[question.id];
                if (picked && picked.length) {
                    kept[question.id] = picked;
                }
            });

            return kept;
        }

        function isAnswerSelected(answers, questionId, optionId) {
            return (answers[questionId] || []).indexOf(optionId) > -1;
        }

        function toggleAnswer(questions, answers, question, optionId) {
            var next = angular.copy(answers);
            var picked = next[question.id] || [];

            if (question.selectType === 'Multi') {
                var index = picked.indexOf(optionId);
                next[question.id] = index > -1
                    ? picked.filter(function (id) { return id !== optionId; })
                    : picked.concat([optionId]);
            } else {
                next[question.id] = [optionId];
            }

            return pruneAnswers(questions, next);
        }

        // The first required question left unanswered, or null. Used to keep the
        // join button disabled rather than letting the API reject the submit.
        function missingRequired(questions, answers) {
            var found = null;

            visibleQuestions(questions, answers).forEach(function (question) {
                if (found) {
                    return;
                }
                var picked = answers[question.id] || [];
                if (question.isRequired && !picked.length) {
                    found = question;
                }
            });

            return found;
        }

        // Only the showing questions' answers go out: the API rejects an answer
        // to a hidden question outright.
        function answerIds(questions, answers) {
            var ids = [];

            visibleQuestions(questions, answers).forEach(function (question) {
                (answers[question.id] || []).forEach(function (optionId) {
                    ids.push(optionId);
                });
            });

            return ids;
        }

        /**
         * Rebuilds the answer map from what the caller already has stored.
         * `myChosenOptions` carries the flat food picks too, so only ids the tree
         * owns are taken.
         */
        function prefill(questions, myChosenOptions) {
            var chosen = myChosenOptions || [];
            var answers = {};

            (questions || []).forEach(function (question) {
                var picked = (question.options || [])
                    .map(function (option) { return option.id; })
                    .filter(function (id) { return chosen.indexOf(id) > -1; });

                if (picked.length) {
                    answers[question.id] = picked;
                }
            });

            // A host may have restructured the branches since this answer was
            // given, stranding a pick on a now-unreachable question.
            return pruneAnswers(questions, answers);
        }
    }
})();
