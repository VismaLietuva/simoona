using Razor.Templating.Core;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.Infrastructure.Email.Attributes;
using Shrooms.Infrastructure.Email.Extensions;
using Shrooms.Infrastructure.Email.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Shrooms.Infrastructure.Email.Templating
{
    public class MailTemplate : IMailTemplate
    {
        private readonly IRazorTemplateEngine _razorTemplateEngine;
        private readonly IApplicationSettings _appSettings;

        public MailTemplate(IRazorTemplateEngine razorTemplateEngine, IApplicationSettings appSettings)
        {
            _razorTemplateEngine = razorTemplateEngine ?? throw new ArgumentNullException(nameof(razorTemplateEngine));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        public async Task<string> GenerateAsync<TEmailTemplate>(TEmailTemplate viewModel, string key, string timeZoneKey = null)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            if (string.IsNullOrEmpty(timeZoneKey))
            {
                return await GenerateInternalAsync(viewModel, key);
            }

            var timeZonePropertiesWithInitialValues = ExtractPropertiesMarkedWithApplyTimeZoneChangesAttribute(viewModel);
            if (!timeZonePropertiesWithInitialValues.Any())
            {
                return await GenerateInternalAsync(viewModel, key);
            }

            return await ApplyTimeZoneChangesToSingleTemplateAsync(viewModel, key, timeZonePropertiesWithInitialValues, timeZoneKey);
        }

        public async Task<ITimeZoneEmailGroup> GenerateAsync<TEmailTemplate>(TEmailTemplate viewModel, string key, IEnumerable<string> timeZoneKeys)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            if (!timeZoneKeys.Any())
            {
                throw new ArgumentException("This method cannot be used without time zone keys.");
            }

            var timeZonePropertiesWithInitialValues = ExtractPropertiesMarkedWithApplyTimeZoneChangesAttribute(viewModel);
            if (!timeZonePropertiesWithInitialValues.Any())
            {
                throw new ArgumentException($"Template {typeof(TEmailTemplate)} does not contain properties that require time zone changes.");
            }

            return new TimeZoneEmailGroup(await ApplyTimeZoneChangesToMultipleTemplatesAsync(viewModel, key, timeZoneKeys, timeZonePropertiesWithInitialValues));
        }

        private async Task<string> ApplyTimeZoneChangesToSingleTemplateAsync<TEmailTemplate>(
            TEmailTemplate viewModel,
            string key,
            List<(PropertyInfo, DateTime)> timeZonePropertiesWithInitialValues,
            string timeZoneKey) where TEmailTemplate : BaseEmailTemplateViewModel
        {
            foreach (var propertyWithInitialValue in timeZonePropertiesWithInitialValues)
            {
                var zonedDate = propertyWithInitialValue.Item2.ConvertUtcToTimeZone(timeZoneKey);
                propertyWithInitialValue.Item1.SetValue(viewModel, zonedDate);
            }

            var compiledTemplate = await GenerateInternalAsync(viewModel, key);
            RestoreInitialValuesToTemplate(viewModel, timeZonePropertiesWithInitialValues);

            return compiledTemplate;
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

        private async Task<Dictionary<string, string>> ApplyTimeZoneChangesToMultipleTemplatesAsync<TEmailTemplate>(
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
                    var zonedDate = propertyWithInitialValue.Item2.ConvertUtcToTimeZone(timeZoneKey);
                    propertyWithInitialValue.Item1.SetValue(viewModel, zonedDate);
                }
                compiledTemplates[timeZoneKey] = await GenerateInternalAsync(viewModel, key);
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

        private async Task<string> GenerateInternalAsync<TEmailTemplate>(TEmailTemplate viewModel, string key)
            where TEmailTemplate : BaseEmailTemplateViewModel
        {
            // The layout's only environment-dependent link, filled here so no caller has to pass it.
            viewModel.HomeUrl = _appSettings.ClientUrl;

            return await _razorTemplateEngine.RenderAsync(key, viewModel);
        }
    }
}
