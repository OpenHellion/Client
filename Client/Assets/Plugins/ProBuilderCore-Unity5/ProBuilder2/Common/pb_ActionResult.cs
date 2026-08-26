namespace ProBuilder2.Common
{
	public class pb_ActionResult
	{
		public Status status;

		public string notification = string.Empty;

		public static pb_ActionResult Success => new pb_ActionResult(Status.Success, string.Empty);

		public static pb_ActionResult NoSelection => new pb_ActionResult(Status.Canceled, "Nothing Selected");

		public static pb_ActionResult UserCanceled => new pb_ActionResult(Status.Canceled, "User Canceled");

		public pb_ActionResult(Status status, string notification)
		{
			this.status = status;
			this.notification = notification;
		}

		public static implicit operator bool(pb_ActionResult res)
		{
			return res.status == Status.Success;
		}
	}
}
