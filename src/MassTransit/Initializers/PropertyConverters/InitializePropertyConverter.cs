namespace MassTransit.Initializers.PropertyConverters
{
    using System.Threading.Tasks;
    using Util;


    public class InitializePropertyConverter<TProperty, TInput> :
        IPropertyConverter<TProperty, TInput>
        where TProperty : class
        where TInput : class
    {
        Task<TProperty> IPropertyConverter<TProperty, TInput>.Convert<TMessage>(InitializeContext<TMessage> context, TInput input)
        {
            if (input == null)
                return TaskUtil.Default<TProperty>();

            InitializeContext<TProperty> messageContext = MessageFactoryCache<TProperty>.Factory.Create(context);

            IMessageInitializer<TProperty> initializer = typeof(TInput) == typeof(object)
                ? MessageInitializerCache<TProperty>.GetInitializer(input.GetType())
                : MessageInitializerCache<TProperty>.GetInitializer(typeof(TInput));

            Task<InitializeContext<TProperty>> initTask = initializer.Initialize(messageContext, input);
            if (initTask.IsCompleted)
                return Task.FromResult(initTask.Result.Message);

            async Task<TProperty> ConvertAsync()
            {
                InitializeContext<TProperty> result = await initTask.ConfigureAwait(false);

                return result.Message;
            }

            return ConvertAsync();
        }
    }
}
