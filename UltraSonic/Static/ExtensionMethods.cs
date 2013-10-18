using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;

namespace UltraSonic.Static
{
    public static class NotificationExtensions
    {
        public static void SubscribeToChange<T>(this T objectThatNotifies, Expression<Func<object>> expression, Action<object> handler) where T : INotifyPropertyChanged
        {
            objectThatNotifies.PropertyChanged += (s, e) =>
                {
                    var lambda = expression as LambdaExpression;
                    MemberExpression memberExpression;

                    if (lambda.Body is UnaryExpression)
                    {
                        UnaryExpression unaryExpression = lambda.Body as UnaryExpression;
                        memberExpression = unaryExpression.Operand as MemberExpression;
                    }
                    else
                    {
                        memberExpression = lambda.Body as MemberExpression;
                    }

                    if (memberExpression == null) return;

                    PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;

                    if (propertyInfo != null && e.PropertyName.Equals(propertyInfo.Name))
                        handler(objectThatNotifies);
                };
        } 

        public static void Notify(this PropertyChangedEventHandler eventHandler, Expression<Func<object>> expression)
        {
            if (null == eventHandler) return;
            
            LambdaExpression lambda = expression;
            MemberExpression memberExpression;

            if (lambda.Body is UnaryExpression)
            {
                UnaryExpression unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            if (memberExpression == null) return;

            var constantExpression = memberExpression.Expression as ConstantExpression;
            var propertyInfo = memberExpression.Member as PropertyInfo;

            foreach (var del in eventHandler.GetInvocationList().Where(del => constantExpression != null).Where(del => propertyInfo != null).Where(del => constantExpression != null))
                if (constantExpression != null && propertyInfo != null)
                    del.DynamicInvoke(new[] {constantExpression.Value, new PropertyChangedEventArgs(propertyInfo.Name)});
        }
    }

    public static class ExtensionMethods
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static void CopyTo(this object s, object T)
        {
            foreach (var pS in s.GetType().GetProperties())
            {
                foreach (var pT in T.GetType().GetProperties())
                {
                    if (pT.Name != pS.Name) continue;
                    (pT.GetSetMethod()).Invoke(T, new[] {pS.GetGetMethod().Invoke(s, null)});
                }
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                int n = list.Count;
                int arraySize = (int) Math.Ceiling((double) n/byte.MaxValue);

                while (n > 1)
                {
                    byte[] box = new byte[arraySize];
                    do provider.GetBytes(box); while (!(box[0] < n*((Byte.MaxValue*arraySize)/n)));
                    int k = (box[0]%n);
                    n--;
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
        }
    }
}