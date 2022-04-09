using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda.Generation.Tools
{
    public class Constructors<TKnownType>
    {
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TKnownType> New<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15), typeof(T16) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var t6 = Expression.Parameter(typeof(T6), "t6");
            var t7 = Expression.Parameter(typeof(T7), "t7");
            var t8 = Expression.Parameter(typeof(T8), "t8");
            var t9 = Expression.Parameter(typeof(T9), "t9");
            var t10 = Expression.Parameter(typeof(T10), "t10");
            var t11 = Expression.Parameter(typeof(T11), "t11");
            var t12 = Expression.Parameter(typeof(T12), "t12");
            var t13 = Expression.Parameter(typeof(T13), "t13");
            var t14 = Expression.Parameter(typeof(T14), "t14");
            var t15 = Expression.Parameter(typeof(T15), "t15");
            var t16 = Expression.Parameter(typeof(T16), "t16");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TKnownType>>(newExpression, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16).Compile();
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TKnownType> New<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14), typeof(T15) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var t6 = Expression.Parameter(typeof(T6), "t6");
            var t7 = Expression.Parameter(typeof(T7), "t7");
            var t8 = Expression.Parameter(typeof(T8), "t8");
            var t9 = Expression.Parameter(typeof(T9), "t9");
            var t10 = Expression.Parameter(typeof(T10), "t10");
            var t11 = Expression.Parameter(typeof(T11), "t11");
            var t12 = Expression.Parameter(typeof(T12), "t12");
            var t13 = Expression.Parameter(typeof(T13), "t13");
            var t14 = Expression.Parameter(typeof(T14), "t14");
            var t15 = Expression.Parameter(typeof(T15), "t15");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TKnownType>>(newExpression, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15).Compile();
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TKnownType> New<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13), typeof(T14) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var t6 = Expression.Parameter(typeof(T6), "t6");
            var t7 = Expression.Parameter(typeof(T7), "t7");
            var t8 = Expression.Parameter(typeof(T8), "t8");
            var t9 = Expression.Parameter(typeof(T9), "t9");
            var t10 = Expression.Parameter(typeof(T10), "t10");
            var t11 = Expression.Parameter(typeof(T11), "t11");
            var t12 = Expression.Parameter(typeof(T12), "t12");
            var t13 = Expression.Parameter(typeof(T13), "t13");
            var t14 = Expression.Parameter(typeof(T14), "t14");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TKnownType>>(newExpression, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14).Compile();
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TKnownType> New<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12), typeof(T13) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var t6 = Expression.Parameter(typeof(T6), "t6");
            var t7 = Expression.Parameter(typeof(T7), "t7");
            var t8 = Expression.Parameter(typeof(T8), "t8");
            var t9 = Expression.Parameter(typeof(T9), "t9");
            var t10 = Expression.Parameter(typeof(T10), "t10");
            var t11 = Expression.Parameter(typeof(T11), "t11");
            var t12 = Expression.Parameter(typeof(T12), "t12");
            var t13 = Expression.Parameter(typeof(T13), "t13");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TKnownType>>(newExpression, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13).Compile();
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TKnownType> New<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11), typeof(T12) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var t6 = Expression.Parameter(typeof(T6), "t6");
            var t7 = Expression.Parameter(typeof(T7), "t7");
            var t8 = Expression.Parameter(typeof(T8), "t8");
            var t9 = Expression.Parameter(typeof(T9), "t9");
            var t10 = Expression.Parameter(typeof(T10), "t10");
            var t11 = Expression.Parameter(typeof(T11), "t11");
            var t12 = Expression.Parameter(typeof(T12), "t12");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TKnownType>>(newExpression, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12).Compile();
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TKnownType> New<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10), typeof(T11) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var t6 = Expression.Parameter(typeof(T6), "t6");
            var t7 = Expression.Parameter(typeof(T7), "t7");
            var t8 = Expression.Parameter(typeof(T8), "t8");
            var t9 = Expression.Parameter(typeof(T9), "t9");
            var t10 = Expression.Parameter(typeof(T10), "t10");
            var t11 = Expression.Parameter(typeof(T11), "t11");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TKnownType>>(newExpression, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11).Compile();
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TKnownType> New<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var t6 = Expression.Parameter(typeof(T6), "t6");
            var t7 = Expression.Parameter(typeof(T7), "t7");
            var t8 = Expression.Parameter(typeof(T8), "t8");
            var t9 = Expression.Parameter(typeof(T9), "t9");
            var t10 = Expression.Parameter(typeof(T10), "t10");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TKnownType>>(newExpression, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10).Compile();
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TKnownType> New<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var t6 = Expression.Parameter(typeof(T6), "t6");
            var t7 = Expression.Parameter(typeof(T7), "t7");
            var t8 = Expression.Parameter(typeof(T8), "t8");
            var t9 = Expression.Parameter(typeof(T9), "t9");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5, t6, t7, t8, t9);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TKnownType>>(newExpression, t1, t2, t3, t4, t5, t6, t7, t8, t9).Compile();
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, T8, TKnownType> New<T1, T2, T3, T4, T5, T6, T7, T8>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var t6 = Expression.Parameter(typeof(T6), "t6");
            var t7 = Expression.Parameter(typeof(T7), "t7");
            var t8 = Expression.Parameter(typeof(T8), "t8");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5, t6, t7, t8);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, T8, TKnownType>>(newExpression, t1, t2, t3, t4, t5, t6, t7, t8).Compile();
        }
        public static Func<T1, T2, T3, T4, T5, T6, T7, TKnownType> New<T1, T2, T3, T4, T5, T6, T7>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var t6 = Expression.Parameter(typeof(T6), "t6");
            var t7 = Expression.Parameter(typeof(T7), "t7");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5, t6, t7);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, T7, TKnownType>>(newExpression, t1, t2, t3, t4, t5, t6, t7).Compile();
        }
        public static Func<T1, T2, T3, T4, T5, T6, TKnownType> New<T1, T2, T3, T4, T5, T6>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var t6 = Expression.Parameter(typeof(T6), "t6");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5, t6);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, T6, TKnownType>>(newExpression, t1, t2, t3, t4, t5, t6).Compile();
        }
        public static Func<T1, T2, T3, T4, T5, TKnownType> New<T1, T2, T3, T4, T5>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var t5 = Expression.Parameter(typeof(T5), "t5");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4, t5);
            return Expression.Lambda<Func<T1, T2, T3, T4, T5, TKnownType>>(newExpression, t1, t2, t3, t4, t5).Compile();
        }
        public static Func<T1, T2, T3, T4, TKnownType> New<T1, T2, T3, T4>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var t4 = Expression.Parameter(typeof(T4), "t4");
            var newExpression = Expression.New(ctor, t1, t2, t3, t4);
            return Expression.Lambda<Func<T1, T2, T3, T4, TKnownType>>(newExpression, t1, t2, t3, t4).Compile();
        }
        public static Func<T1, T2, T3, TKnownType> New<T1, T2, T3>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var t3 = Expression.Parameter(typeof(T3), "t3");
            var newExpression = Expression.New(ctor, t1, t2, t3);
            return Expression.Lambda<Func<T1, T2, T3, TKnownType>>(newExpression, t1, t2, t3).Compile();
        }
        public static Func<T1, T2, TKnownType> New<T1, T2>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1), typeof(T2) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var t2 = Expression.Parameter(typeof(T2), "t2");
            var newExpression = Expression.New(ctor, t1, t2);
            return Expression.Lambda<Func<T1, T2, TKnownType>>(newExpression, t1, t2).Compile();
        }
        public static Func<T1, TKnownType> New<T1>(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, new Type[] { typeof(T1) }, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var t1 = Expression.Parameter(typeof(T1), "t1");
            var newExpression = Expression.New(ctor, t1);
            return Expression.Lambda<Func<T1, TKnownType>>(newExpression, t1).Compile();
        }
        public static Func<TKnownType> New(Type t, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));
            if (!typeof(TKnownType).IsAssignableFrom(t)) throw new ArgumentException($"{nameof(TKnownType)} cannot be assigned to from the type provided by {nameof(t)}.", nameof(t));
            var ctor = t.GetConstructor(bindingFlags, Type.DefaultBinder, Type.EmptyTypes, null);
            if (ctor == null)
                throw new ArgumentException(null, nameof(t));
            var newExpression = Expression.New(ctor);
            return Expression.Lambda<Func<TKnownType>>(newExpression).Compile();
        }
    }
}
