﻿using RazorEngine;
using RazorEngine.Templating;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure.Email;

namespace Shrooms.Infrastructure.Email.Templating
{
    public class MailTemplate : IMailTemplate
    {
        public string Generate<T>(T viewModel, string key)
            where T : BaseEmailTemplateViewModel
        {
            return Engine.Razor.Run(key, typeof(T), viewModel);
        }
    }
}
