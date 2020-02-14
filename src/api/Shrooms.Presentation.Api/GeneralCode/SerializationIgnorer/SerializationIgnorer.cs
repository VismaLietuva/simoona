using System;
using System.Collections;
using System.Collections.Generic;

namespace Shrooms.Presentation.Api.GeneralCode.SerializationIgnorer
{
    public static class SerializationIgnorer
    {
        private const int MaxDepth = 10;

        private static readonly Dictionary<Type, Dictionary<Type, List<string>>> _configurationDictionary = new Dictionary<Type, Dictionary<Type, List<string>>>();

        public static void IgnoreProperty<TViewModel>(Type typeFrom, string memberName)
        {
            if (!_configurationDictionary.ContainsKey(typeof(TViewModel)))
            {
                _configurationDictionary.Add(typeof(TViewModel), new Dictionary<Type, List<string>>());
            }

            if (!_configurationDictionary[typeof(TViewModel)].ContainsKey(typeFrom))
            {
                _configurationDictionary[typeof(TViewModel)].Add(typeFrom, new List<string>());
            }

            _configurationDictionary[typeof(TViewModel)][typeFrom].Add(memberName);
        }

        public static SerializationIgnorerExpression<TViewModel> Create<TViewModel>()
        {
            return new SerializationIgnorerExpression<TViewModel>();
        }

        public static void ModifyViewModel(object viewModel)
        {
            if (viewModel == null)
            {
                return;
            }

            // If viewModel is IEnumerable then modify every array item
            if (viewModel is IEnumerable viewModelList)
            {
                foreach (var viewModelIterator in viewModelList)
                {
                    ModifyViewModel(viewModelIterator);
                }

                return;
            }

            var propertiesToIgnore = GetIgnores(viewModel.GetType());
            if (propertiesToIgnore == null)
            {
                return;
            }

            var depthCounter = 0;
            ModifyObject(propertiesToIgnore, viewModel, ref depthCounter);
        }

        #region Private methods

        private static void ModifyObject(Dictionary<Type, List<string>> propertiesToIgnore, object viewModel, ref int depthCounter)
        {
            if (depthCounter >= MaxDepth)
            {
                return;
            }

            var properties = viewModel.GetType().GetProperties();
            foreach (var propertyInfo in properties)
            {
                var propertyValue = propertyInfo.GetValue(viewModel);

                // If it's ignored or max depth reached
                if (propertiesToIgnore.ContainsKey(viewModel.GetType()) && propertiesToIgnore[viewModel.GetType()].Contains(propertyInfo.Name) && propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(viewModel, null);
                    continue;
                }

                // If it's an array
                if (propertyValue is IEnumerable && !(propertyValue is string))
                {
                    depthCounter++;
                    ModifyArray(propertiesToIgnore, propertyValue, ref depthCounter);
                    continue;
                }

                // If it's an object
                if (propertyInfo.PropertyType.IsClass && propertyValue != null && !(propertyValue is string))
                {
                    depthCounter++;
                    ModifyObject(propertiesToIgnore, propertyValue, ref depthCounter);
                }
            }

            depthCounter--;
        }

        private static void ModifyArray(Dictionary<Type, List<string>> propertiesToIgnore, object viewModel, ref int depthCounter)
        {
            var viewModelList = (IEnumerable)viewModel;
            foreach (var viewModelIterator in viewModelList)
            {
                ModifyObject(propertiesToIgnore, viewModelIterator, ref depthCounter);
            }
        }

        private static Dictionary<Type, List<string>> GetIgnores(Type type)
        {
            if (!_configurationDictionary.ContainsKey(type))
            {
                return null;
            }

            //throw new ServerException(string.Format("There is no '{0}' defined in SerializationIgnorer.", typeof(TViewModel)));
            return _configurationDictionary[type];
        }

        #endregion
    }
}