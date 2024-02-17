using InputInterceptorNS;

namespace Script_Engine.Utilities
{
	public static class Interception
	{
		public static MouseHook Mouse;
		public static bool Run()
		{
			if (Load())
			{
				Mouse = new(null);
				return true;
			}
			return false;
		}

		public static bool Load()
		{
			if (InputInterceptor.CheckDriverInstalled())
			{
				if (InputInterceptor.Initialize())
				{
					return true;
				}
			}
			return false;
		}

		public static void Unload()
		{
			Mouse?.Dispose();
			InputInterceptor.Dispose();
		}
	}
}
