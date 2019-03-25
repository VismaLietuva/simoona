namespace Shrooms.API.GeneralCode.SerializationIgnorer
{
    using System;
    using System.Linq.Expressions;

    public class SerializationIgnorerExpression<TRootViewModel>
    {
        public SerializationIgnorerExpression<TRootViewModel> ForMember<TViewModel>(
            Expression<Func<TViewModel, object>> destinationMember)
        {
            var member = destinationMember.Body as MemberExpression;

            SerializationIgnorer.IgnoreProperty<TRootViewModel>(typeof(TViewModel), member.Member.Name);
            return new SerializationIgnorerExpression<TRootViewModel>();
        }
    }
}