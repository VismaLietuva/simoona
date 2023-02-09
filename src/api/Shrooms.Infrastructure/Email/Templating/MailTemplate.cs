using RazorEngine;
using RazorEngine.Templating;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Shrooms.Infrastructure.Email.Extensions;
using Shrooms.Infrastructure.Email.Models;

namespace Shrooms.Infrastructure.Email.Templating
{
    public class MailTemplate : IMailTemplate
    {
        public string Generate<TEmailTemplate>(TEmailTemplate viewModel, string key, string timeZoneKey = null)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            if (string.IsNullOrEmpty(timeZoneKey))
            {
                return GenerateInternal(viewModel, key);
            }

            var timeZonePropertiesWithInitialValues = ExtractPropertiesMarkedWithApplyTimeZoneChangesAttribute(viewModel);
            if (!timeZonePropertiesWithInitialValues.Any())
            {
                return GenerateInternal(viewModel, key);
            }

            return ApplyTimeZoneChangesToSingleTemplate(viewModel, key, timeZonePropertiesWithInitialValues, timeZoneKey);
        }

        public ITimeZoneEmailGroup Generate<TEmailTemplate>(
            TEmailTemplate viewModel,
            string key,
            IEnumerable<string> timeZoneKeys)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            if (!timeZoneKeys.Any())
            {
                throw new ArgumentException($"This method cannot be used without time zone keys.");
            }

            var timeZonePropertiesWithInitialValues = ExtractPropertiesMarkedWithApplyTimeZoneChangesAttribute(viewModel);
            if (!timeZonePropertiesWithInitialValues.Any())
            {
                throw new ArgumentException($"Template {typeof(TEmailTemplate)} does not contain properties that require time zone changes.");
            }

            return new TimeZoneEmailGroup(ApplyTimeZoneChangesToMultipleTemplates(viewModel, key, timeZoneKeys, timeZonePropertiesWithInitialValues));
        }

        private string ApplyTimeZoneChangesToSingleTemplate<TEmailTemplate>(
            TEmailTemplate viewModel,
            string key,
            List<(PropertyInfo, DateTime)> timeZonePropertiesWithInitialValues,
            string timeZoneKey) where TEmailTemplate : BaseEmailTemplateViewModel
        {
            foreach (var propertyWithInitialValue in timeZonePropertiesWithInitialValues)
            {
                ApplyTimeZoneChangesToProperty(viewModel, timeZoneKey, propertyWithInitialValue);
            }
            var compiledTemplate = GenerateInternal(viewModel, key);
            RestoreInitialValuesToTemplate(viewModel, timeZonePropertiesWithInitialValues);

            return compiledTemplate;
        }

        private static void ApplyTimeZoneChangesToProperty<TEmailTemplate>(
            TEmailTemplate viewModel,
            string timeZoneKey,
            (PropertyInfo, DateTime) propertyWithInitialValue)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            var zonedDate = propertyWithInitialValue.Item2.ConvertUtcToTimeZone(timeZoneKey);
            propertyWithInitialValue.Item1.SetValue(viewModel, zonedDate);
        }

        private static void RestoreInitialValuesToTemplate<TEmailTemplate>(
            TEmailTemplate viewModel,
            List<(PropertyInfo, DateTime)> timeZonePropertiesWithInitialValues)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            foreach (var propertyWithInitialValue in timeZonePropertiesWithInitialValues)
            {
                propertyWithInitialValue.Item1.SetValue(viewModel, propertyWithInitialValue.Item2);
            }
        }

        private Dictionary<string, string> ApplyTimeZoneChangesToMultipleTemplates<TEmailTemplate>(
            TEmailTemplate viewModel,
            string key,
            IEnumerable<string> timeZoneKeys,
            List<(PropertyInfo, DateTime)> timeZonePropertiesWithInitialValues)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            var compiledTemplates = new Dictionary<string, string>();
            foreach (var timeZoneKey in timeZoneKeys.Distinct())
            {
                foreach (var propertyWithInitialValue in timeZonePropertiesWithInitialValues)
                {
                    ApplyTimeZoneChangesToProperty(viewModel, timeZoneKey, propertyWithInitialValue);
                }
                compiledTemplates[timeZoneKey] = GenerateInternal(viewModel, key);
            }
            RestoreInitialValuesToTemplate(viewModel, timeZonePropertiesWithInitialValues);
            return compiledTemplates;
        }

        private static List<(PropertyInfo, DateTime)> ExtractPropertiesMarkedWithApplyTimeZoneChangesAttribute<TEmailTemplate>(TEmailTemplate viewModel)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            var templateType = viewModel.GetType();
            var timeZonePropertiesWithInitialValues = new List<(PropertyInfo, DateTime)>();
            foreach (var property in templateType.GetProperties())
            {
                var attribute = property.GetCustomAttribute(typeof(ApplyTimeZoneChangesAttribute));
                if (attribute == null)
                {
                    continue;
                }
                timeZonePropertiesWithInitialValues.Add((property, (DateTime)property.GetValue(viewModel)));
            }

            return timeZonePropertiesWithInitialValues;
        }

        private string GenerateInternal<TEmailTemplate>(TEmailTemplate viewModel, string key)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            return Engine.Razor.Run(key, typeof(TEmailTemplate), viewModel);
        }
    }
}
