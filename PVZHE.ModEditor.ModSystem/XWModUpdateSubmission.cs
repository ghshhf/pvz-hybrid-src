using System.Threading.Tasks;

namespace PVZHE.ModEditor.ModSystem;

internal sealed class XWModUpdateSubmission(XWModUpdateService service, XWModUpdateNotice notice)
{
	internal XWModUpdateService Service { get; } = service;

	internal XWModUpdateNotice Notice { get; } = notice;

	internal TaskCompletionSource<XWModCommandResult<bool>> Completion { get; } = new TaskCompletionSource<XWModCommandResult<bool>>(TaskCreationOptions.RunContinuationsAsynchronously);
}
