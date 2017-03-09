using Ninject;

namespace IocContainer
{

    public static class IoCContainer
    {
        private static IKernel _kernel;
        public static IKernel Kernel
        {
            get
            {
                return _kernel;

            }
            set { _kernel = value; }
        }

    }
}
