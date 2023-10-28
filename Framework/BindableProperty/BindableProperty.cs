using System;
namespace FrameworkDesign
{
    //数据和数据变更事件的泛型合集
    public class BindableProperty<T> 
    {
        private Action<T> onValueChange = e => { };

        private T mValue = default(T);
        public T Value
        {
            get => mValue;
            set
            {
                if (!value.Equals(mValue))
                {
                    mValue = value;
                    onValueChange?.Invoke(value);
                }
            }

        }

        //封装BindableProperty的onValueChange
        public IUnRegister RegisterOnValueChange(Action<T> action)
        {
            onValueChange += action;

            return new BindablePropertyUnRegister<T>()
            {
                BindableProperty = this,
                OnValueChange = onValueChange
            };
        }
        public void UnRegisterOnValueChange(Action<T> action) 
        {
            onValueChange -= action;
        }


        //给BindableProperty封装事件系统
        public class BindablePropertyUnRegister<T> : IUnRegister 
        {
            public BindableProperty<T> BindableProperty { get; set; }

            public Action<T> OnValueChange { get; set; }

            public void UnRegister()
            {
                BindableProperty.UnRegisterOnValueChange(OnValueChange);
                BindableProperty = null;
                OnValueChange = null;
            }
        }

    }
}
