using System.Collections.Generic;

namespace Verse
{
	public static class DelayedErrorWindowRequest
	{
		private struct Request
		{
			public string text;

			public string title;
		}

		private static List<Request> requests = new List<Request>();

		public static void DelayedErrorWindowRequestOnGUI()
		{
			try
			{
				for (int i = 0; i < DelayedErrorWindowRequest.requests.Count; i++)
				{
					WindowStack windowStack = Find.WindowStack;
					Request request = DelayedErrorWindowRequest.requests[i];
					string title = request.title;
					Request request2 = DelayedErrorWindowRequest.requests[i];
					windowStack.Add(new Dialog_MessageBox(request2.text, "OK".Translate(), null, (string)null, null, title, false));
				}
			}
			finally
			{
				DelayedErrorWindowRequest.requests.Clear();
			}
		}

		public static void Add(string text, string title = null)
		{
			Request item = new Request
			{
				text = text,
				title = title
			};
			DelayedErrorWindowRequest.requests.Add(item);
		}
	}
}