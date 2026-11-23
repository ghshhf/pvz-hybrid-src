using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
[ScriptPath("res://Script/Component/ComponentManager.cs")]
public class ComponentManager : Resource
{
	private sealed class RuntimeAttachment
	{
		public CharacterComponentRuntime Runtime;

		public TowerDefenseCharacter Owner;

		public string InstanceId;

		public string TypeName;

		public string WireKey;

		public ComponentRegistration Registration;

		public string PreviousInstanceId;

		public string PreviousDefinitionId;

		public string PreviousWireKey;

		public long PreviousRevision;

		public long ReservedRevision;

		public int PreviousSlotCount;

		public List<(ComponentRegistration Slot, long Revision)> AppendedSlots;

		public Dictionary PendingState;
	}

	private sealed class ComponentRegistration
	{
		public string TypeName;

		public int TypeIndex;

		public WeakReference<ComponentBase> ActiveComponent;

		public CharacterComponentRuntime ActiveRuntime;

		public string InstanceId;

		public string DefinitionId;

		public long IdentityRevision;

		public bool HasProcessWork;

		public bool HasPhysicsWork;

		public bool HasInputWork;

		public bool HasUnhandledInputWork;

		public int PhysicsStatefulIndex = -1;

		public string WireKey;
	}

	public new class MethodName : Resource.MethodName
	{
		public static readonly StringName CancelAttachmentTransaction = "CancelAttachmentTransaction";

		public static readonly StringName CaptureAttachmentNetworkState = "CaptureAttachmentNetworkState";

		public static readonly StringName IsInsideTree = "IsInsideTree";

		public static readonly StringName GetParent = "GetParent";

		public static readonly StringName AddChild = "AddChild";

		public static readonly StringName RemoveChild = "RemoveChild";

		public static readonly StringName AttachOwner = "AttachOwner";

		public static readonly StringName DetachOwner = "DetachOwner";

		public static readonly StringName Release = "Release";

		public new static readonly StringName _Notification = "_Notification";

		public static readonly StringName InitializeResourceComponents = "InitializeResourceComponents";

		public static readonly StringName ActivateResourceComponents = "ActivateResourceComponents";

		public static readonly StringName NotifyOwnerGameplayActivated = "NotifyOwnerGameplayActivated";

		public static readonly StringName NotifyOwnerBeforeDestroy = "NotifyOwnerBeforeDestroy";

		public static readonly StringName ScheduleResourceStateRegistration = "ScheduleResourceStateRegistration";

		public static readonly StringName RegisterResourceStateRuntimes = "RegisterResourceStateRuntimes";

		public static readonly StringName RebindResourceComponents = "RebindResourceComponents";

		public static readonly StringName DetachResourceComponents = "DetachResourceComponents";

		public static readonly StringName ReleaseResourceComponents = "ReleaseResourceComponents";

		public static readonly StringName RegisterExistingChildren = "RegisterExistingChildren";

		public static readonly StringName OnChildEnteredTree = "OnChildEnteredTree";

		public static readonly StringName OnChildExitingTree = "OnChildExitingTree";

		public static readonly StringName RegisterComponent = "RegisterComponent";

		public static readonly StringName UnregisterComponent = "UnregisterComponent";

		public static readonly StringName AttachRegisteredComponent = "AttachRegisteredComponent";

		public static readonly StringName ApplyOrQueueNetworkState = "ApplyOrQueueNetworkState";

		public static readonly StringName RefreshStateMachineRegistration = "RefreshStateMachineRegistration";

		public static readonly StringName NotifyComponentAliveChanged = "NotifyComponentAliveChanged";

		public static readonly StringName DispatchRuntimeInput = "DispatchRuntimeInput";

		public static readonly StringName _Input = "_Input";

		public static readonly StringName DispatchRuntimeUnhandledInput = "DispatchRuntimeUnhandledInput";

		public static readonly StringName _UnhandledInput = "_UnhandledInput";

		public static readonly StringName TickStateMachineProcess = "TickStateMachineProcess";

		public static readonly StringName TickStateMachinePhysics = "TickStateMachinePhysics";

		public static readonly StringName GetComponentStatePhysicsMetricName = "GetComponentStatePhysicsMetricName";

		public static readonly StringName TickRuntimePhysics = "TickRuntimePhysics";

		public static readonly StringName TickComponentTimers = "TickComponentTimers";

		public static readonly StringName StartComponentTimer = "StartComponentTimer";

		public static readonly StringName StopComponentTimer = "StopComponentTimer";

		public static readonly StringName PauseComponentTimer = "PauseComponentTimer";

		public static readonly StringName ResumeComponentTimer = "ResumeComponentTimer";

		public static readonly StringName IsComponentTimerRunning = "IsComponentTimerRunning";

		public static readonly StringName GetComponentTimerRemaining = "GetComponentTimerRemaining";

		public static readonly StringName RegisterWireSlot = "RegisterWireSlot";

		public static readonly StringName BuildWireKey = "BuildWireKey";

		public static readonly StringName DeactivateWireSlot = "DeactivateWireSlot";

		public static readonly StringName RemovePhysicsRuntimeAtSwap = "RemovePhysicsRuntimeAtSwap";

		public static readonly StringName RemoveStatefulRuntimeAtSwap = "RemoveStatefulRuntimeAtSwap";

		public static readonly StringName RemovePhysicsStatefulRuntimeAtSwap = "RemovePhysicsStatefulRuntimeAtSwap";

		public static readonly StringName RemoveStatefulComponent = "RemoveStatefulComponent";

		public static readonly StringName RemoveStatefulAtSwap = "RemoveStatefulAtSwap";

		public static readonly StringName RefreshStateMachineWorkRegistration = "RefreshStateMachineWorkRegistration";

		public static readonly StringName ClearStateMachineWorkRegistration = "ClearStateMachineWorkRegistration";

		public static readonly StringName RemovePhysicsStatefulComponentAtSwap = "RemovePhysicsStatefulComponentAtSwap";

		public static readonly StringName RefreshOwnerProcessMembership = "RefreshOwnerProcessMembership";

		public static readonly StringName RefreshInputProcessMode = "RefreshInputProcessMode";

		public static readonly StringName GetComponentTypeName = "GetComponentTypeName";

		public static readonly StringName GetComponentFromName = "GetComponentFromName";

		public static readonly StringName GetComponentFromType = "GetComponentFromType";

		public static readonly StringName FindComponentByNodeName = "FindComponentByNodeName";

		public static readonly StringName ResolveLegacyComponentName = "ResolveLegacyComponentName";
	}

	public new class PropertyName : Resource.PropertyName
	{
		public static readonly StringName ComponentSet = "ComponentSet";

		public static readonly StringName Owner = "Owner";

		public static readonly StringName IsOwnerInsideTree = "IsOwnerInsideTree";

		public static readonly StringName Name = "Name";

		public static readonly StringName ProcessMode = "ProcessMode";

		public static readonly StringName HasStateMachineProcessWork = "HasStateMachineProcessWork";

		public static readonly StringName HasStateMachinePhysicsWork = "HasStateMachinePhysicsWork";

		public static readonly StringName HasRuntimePhysicsWork = "HasRuntimePhysicsWork";

		public static readonly StringName HasComponentTimerWork = "HasComponentTimerWork";

		public static readonly StringName HasRuntimeInputWork = "HasRuntimeInputWork";

		public static readonly StringName HasRuntimeUnhandledInputWork = "HasRuntimeUnhandledInputWork";

		public static readonly StringName CanDispatchOwnerGameplay = "CanDispatchOwnerGameplay";

		public static readonly StringName _attachmentOwner = "_attachmentOwner";

		public static readonly StringName _resourceComponentSet = "_resourceComponentSet";

		public static readonly StringName _processStateMachineCount = "_processStateMachineCount";

		public static readonly StringName _physicsStateMachineCount = "_physicsStateMachineCount";

		public static readonly StringName _inputRuntimeCount = "_inputRuntimeCount";

		public static readonly StringName _unhandledInputRuntimeCount = "_unhandledInputRuntimeCount";

		public static readonly StringName _isExitingTree = "_isExitingTree";

		public static readonly StringName _resourceStateRegistrationScheduled = "_resourceStateRegistrationScheduled";

		public static readonly StringName _ownerAttached = "_ownerAttached";

		public static readonly StringName parent = "parent";
	}

	public new class SignalName : Resource.SignalName
	{
	}

	private CharacterComponentAttachmentTransaction _attachmentTransaction;

	private TowerDefenseCharacter _attachmentOwner;

	private readonly List<RuntimeAttachment> _attachments = new List<RuntimeAttachment>();

	private static readonly System.Collections.Generic.Dictionary<string, string> LegacyComponentAliases = new System.Collections.Generic.Dictionary<string, string>(StringComparer.Ordinal)
	{
		["ImpThrowComponent"] = "ImpFlightComponent",
		["ThrowImpComponent"] = "ImpThrowerComponent",
		["WaterComponent"] = "WaterEnvironmentComponent",
		["ReverseInjuryComponent"] = "PeriodicAreaEventComponent"
	};

	public List<ComponentBase> componentList = new List<ComponentBase>();

	private readonly HashSet<ComponentBase> _componentSet = new HashSet<ComponentBase>(ReferenceEqualityComparer.Instance);

	public System.Collections.Generic.Dictionary<string, List<ComponentBase>> componentDictionary = new System.Collections.Generic.Dictionary<string, List<ComponentBase>>();

	private readonly List<CharacterComponentRuntime> _resourceComponents = new List<CharacterComponentRuntime>();

	private readonly List<CharacterComponentRuntime> _ownerGameplayActivationRuntimes = new List<CharacterComponentRuntime>();

	private CharacterComponentRuntime _singleOwnerGameplayActivationRuntime;

	private readonly System.Collections.Generic.Dictionary<CharacterComponentRuntime, ComponentRegistration> _registrationByRuntime = new System.Collections.Generic.Dictionary<CharacterComponentRuntime, ComponentRegistration>(ReferenceEqualityComparer.Instance);

	private readonly System.Collections.Generic.Dictionary<string, CharacterComponentRuntime> _runtimeByInstanceId = new System.Collections.Generic.Dictionary<string, CharacterComponentRuntime>(StringComparer.Ordinal);

	private CharacterComponentSet _resourceComponentSet;

	private CharacterComponentCreationPlan _appliedCreationPlan;

	private readonly List<ComponentBase> _statefulComponents = new List<ComponentBase>();

	private readonly System.Collections.Generic.Dictionary<ComponentBase, int> _statefulIndices = new System.Collections.Generic.Dictionary<ComponentBase, int>(ReferenceEqualityComparer.Instance);

	private readonly List<CharacterComponentRuntime> _statefulRuntimes = new List<CharacterComponentRuntime>();

	private readonly System.Collections.Generic.Dictionary<CharacterComponentRuntime, int> _statefulRuntimeIndices = new System.Collections.Generic.Dictionary<CharacterComponentRuntime, int>(ReferenceEqualityComparer.Instance);

	private List<ComponentBase> _physicsStatefulComponents;

	private List<CharacterComponentRuntime> _physicsStatefulRuntimes;

	private readonly List<CharacterComponentRuntime> _physicsRuntimes = new List<CharacterComponentRuntime>();

	private readonly System.Collections.Generic.Dictionary<CharacterComponentRuntime, int> _physicsRuntimeIndices = new System.Collections.Generic.Dictionary<CharacterComponentRuntime, int>(ReferenceEqualityComparer.Instance);

	private System.Collections.Generic.Dictionary<Type, string> _componentStatePhysicsMetricNames;

	private System.Collections.Generic.Dictionary<Type, string> _runtimeStatePhysicsMetricNames;

	private System.Collections.Generic.Dictionary<Type, string> _runtimePhysicsMetricNames;

	private int _processStateMachineCount;

	private int _physicsStateMachineCount;

	private int _inputRuntimeCount;

	private int _unhandledInputRuntimeCount;

	private readonly System.Collections.Generic.Dictionary<ComponentBase, ComponentRegistration> _registrationByComponent = new System.Collections.Generic.Dictionary<ComponentBase, ComponentRegistration>(ReferenceEqualityComparer.Instance);

	private readonly System.Collections.Generic.Dictionary<string, List<ComponentRegistration>> _slotsByType = new System.Collections.Generic.Dictionary<string, List<ComponentRegistration>>(StringComparer.Ordinal);

	private System.Collections.Generic.Dictionary<string, Dictionary> _pendingNetworkStateByWireKey;

	private ComponentTimerService _timerService;

	private bool _isExitingTree;

	private bool _resourceStateRegistrationScheduled;

	private bool _ownerAttached;

	public Node parent;

	[Export(PropertyHint.None, "")]
	public CharacterComponentSet ComponentSet { get; set; }

	public IReadOnlyList<CharacterComponentRuntime> ResourceComponents => _resourceComponents;

	private TowerDefenseCharacter Owner => parent as TowerDefenseCharacter;

	private bool IsOwnerInsideTree
	{
		get
		{
			if (GodotObject.IsInstanceValid(Owner))
			{
				return Owner.IsInsideTree();
			}
			return false;
		}
	}

	public StringName Name { get; set; } = "ComponentManager";

	public Node.ProcessModeEnum ProcessMode
	{
		get
		{
			if (!GodotObject.IsInstanceValid(Owner))
			{
				return Node.ProcessModeEnum.Inherit;
			}
			return Owner.ProcessMode;
		}
		set
		{
			if (GodotObject.IsInstanceValid(Owner))
			{
				Owner.ProcessMode = value;
			}
		}
	}

	internal bool HasStateMachineProcessWork => _processStateMachineCount > 0;

	internal bool HasStateMachinePhysicsWork => _physicsStateMachineCount > 0;

	internal bool HasRuntimePhysicsWork => _physicsRuntimes.Count > 0;

	internal bool HasComponentTimerWork => _timerService?.HasActiveTimers ?? false;

	internal bool HasRuntimeInputWork => _inputRuntimeCount > 0;

	internal bool HasRuntimeUnhandledInputWork => _unhandledInputRuntimeCount > 0;

	internal bool CanDispatchOwnerGameplay
	{
		get
		{
			if (parent is TowerDefenseCharacter towerDefenseCharacter)
			{
				return towerDefenseCharacter.IsInsideComponentBattlefield;
			}
			return true;
		}
	}

	public CharacterComponentAttachmentTransaction BeginRuntimeComponentAttachment()
	{
		if (_attachmentTransaction != null)
		{
			return new CharacterComponentAttachmentTransaction(null, "This manager already has an open attachment transaction.");
		}
		if (!GodotObject.IsInstanceValid(Owner) || _isExitingTree)
		{
			return new CharacterComponentAttachmentTransaction(null, "An attachment transaction requires a live owner.");
		}
		_attachmentOwner = Owner;
		_attachmentTransaction = new CharacterComponentAttachmentTransaction(this);
		return _attachmentTransaction;
	}

	internal CharacterComponentAttachResult AttachInTransaction(CharacterComponentAttachmentTransaction transaction, CharacterComponentDefinition definition)
	{
		string text = ValidateAttachmentTransaction(transaction);
		if (text.Length > 0)
		{
			return new CharacterComponentAttachResult(null, "", text);
		}
		string id = definition?.InstanceId?.Trim();
		if (string.IsNullOrEmpty(id) || _attachments.Any((RuntimeAttachment item) => item.InstanceId == id) || TryGetRuntimeByInstanceId(id, out var _))
		{
			return new CharacterComponentAttachResult(null, "", "An attachment needs a unique, unoccupied InstanceId: " + id);
		}
		RuntimeAttachment runtimeAttachment = new RuntimeAttachment
		{
			InstanceId = id,
			Owner = Owner
		};
		_attachments.Add(runtimeAttachment);
		try
		{
			AttachRuntimeComponentCore(definition, runtimeAttachment);
			text = ValidateAttachmentTransaction(transaction);
			if (text.Length > 0)
			{
				throw new InvalidOperationException(text);
			}
			return new CharacterComponentAttachResult(runtimeAttachment.Runtime, runtimeAttachment.WireKey, "");
		}
		catch (Exception ex)
		{
			return new CharacterComponentAttachResult(null, "", "Cannot attach '" + id + "': " + ex.GetBaseException().Message);
		}
	}

	internal CharacterComponentAttachmentResult CommitAttachmentTransaction(CharacterComponentAttachmentTransaction transaction)
	{
		string text = ValidateAttachmentTransaction(transaction);
		if (text.Length == 0)
		{
			foreach (RuntimeAttachment attachment in _attachments)
			{
				if (!IsRuntimeManaged(attachment.Runtime, attachment.InstanceId))
				{
					text = "Component '" + attachment.InstanceId + "' was removed before commit.";
					break;
				}
			}
		}
		if (text.Length > 0)
		{
			return transaction.Rollback(text);
		}
		return CompleteAttachmentTransaction(transaction, new CharacterComponentAttachmentResult(CharacterComponentAttachmentStatus.Committed, "", System.Array.Empty<string>(), System.Array.Empty<string>()));
	}

	internal CharacterComponentAttachmentResult RollbackAttachmentTransaction(CharacterComponentAttachmentTransaction transaction, string reason)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string> { reason };
		bool flag = _attachments.Any((RuntimeAttachment item) => item.Runtime != null);
		for (int num = _attachments.Count - 1; num >= 0; num--)
		{
			RuntimeAttachment runtimeAttachment = _attachments[num];
			try
			{
				if (runtimeAttachment.Runtime != null && !RemoveRuntimeComponent(runtimeAttachment.Runtime))
				{
					runtimeAttachment.Runtime.Release(ComponentDetachReason.DefinitionReplaced);
					DeactivateRuntimeWireSlot(runtimeAttachment.Runtime);
				}
			}
			catch (Exception ex)
			{
				list3.Add(ex.GetBaseException().Message);
			}
			bool flag2 = IsAttachmentRuntimeClean(runtimeAttachment.Runtime);
			if (!flag2)
			{
				list2.Add(runtimeAttachment.InstanceId);
			}
			try
			{
				RestoreAttachmentPendingState(runtimeAttachment);
				if (!flag2 || !RestoreAttachmentIdentity(runtimeAttachment))
				{
					list.Add(runtimeAttachment.InstanceId);
				}
			}
			catch (Exception ex2)
			{
				list2.Add(runtimeAttachment.InstanceId);
				list3.Add(ex2.GetBaseException().Message);
			}
		}
		CharacterComponentAttachmentStatus status = ((list2.Count > 0) ? CharacterComponentAttachmentStatus.CleanupIncomplete : ((list.Count > 0) ? CharacterComponentAttachmentStatus.IdentityRetained : (flag ? CharacterComponentAttachmentStatus.RolledBack : CharacterComponentAttachmentStatus.Rejected)));
		return CompleteAttachmentTransaction(transaction, new CharacterComponentAttachmentResult(status, string.Join("\n", list3), System.Array.AsReadOnly(list.Distinct().ToArray()), System.Array.AsReadOnly(list2.Distinct().ToArray())));
	}

	private string ValidateAttachmentTransaction(CharacterComponentAttachmentTransaction transaction)
	{
		if (_attachmentTransaction != transaction)
		{
			return "Attachment transaction is no longer current.";
		}
		if (!string.IsNullOrEmpty(transaction.CancellationReason))
		{
			return transaction.CancellationReason;
		}
		if (!GodotObject.IsInstanceValid(_attachmentOwner) || Owner != _attachmentOwner || _isExitingTree)
		{
			return "The attachment owner was detached or released.";
		}
		return "";
	}

	private CharacterComponentAttachmentResult CompleteAttachmentTransaction(CharacterComponentAttachmentTransaction transaction, CharacterComponentAttachmentResult result)
	{
		_attachments.Clear();
		_attachmentOwner = null;
		_attachmentTransaction = null;
		transaction.Complete(result);
		return result;
	}

	private void CancelAttachmentTransaction()
	{
		CharacterComponentAttachmentTransaction attachmentTransaction = _attachmentTransaction;
		if (attachmentTransaction != null)
		{
			attachmentTransaction.CancellationReason = "The attachment owner was detached or released.";
			if (!attachmentTransaction.Executing)
			{
				attachmentTransaction.Rollback(attachmentTransaction.CancellationReason);
			}
		}
	}

	private bool IsRuntimeManaged(CharacterComponentRuntime runtime, string instanceId)
	{
		if (runtime != null && !runtime.IsReleased && runtime.Manager == this && runtime.Owner == Owner && _resourceComponents.Contains(runtime) && _runtimeByInstanceId.TryGetValue(instanceId, out var value) && value == runtime && _registrationByRuntime.TryGetValue(runtime, out var value2))
		{
			return value2.ActiveRuntime == runtime;
		}
		return false;
	}

	private bool IsAttachmentRuntimeClean(CharacterComponentRuntime runtime)
	{
		if (runtime != null)
		{
			if (runtime.IsReleased && !runtime.HasModLease && runtime.Manager == null && runtime.Owner == null && !_resourceComponents.Contains(runtime) && !_registrationByRuntime.ContainsKey(runtime) && !_runtimeByInstanceId.Values.Contains(runtime) && !_physicsRuntimeIndices.ContainsKey(runtime) && !_statefulRuntimeIndices.ContainsKey(runtime))
			{
				return !_ownerGameplayActivationRuntimes.Contains(runtime);
			}
			return false;
		}
		return true;
	}

	private void AttachRuntimeComponentCore(CharacterComponentDefinition definition, RuntimeAttachment attachment)
	{
		if (!GodotObject.IsInstanceValid(definition) || !GodotObject.IsInstanceValid(Owner) || _isExitingTree)
		{
			throw new InvalidOperationException("A live owner and component definition are required.");
		}
		string text = ResolveLegacyComponentName(definition.ComponentTypeId?.Trim());
		string instanceId = definition.InstanceId?.Trim();
		if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(instanceId))
		{
			throw new InvalidOperationException("Stable ComponentTypeId and InstanceId values are required.");
		}
		attachment.Owner = Owner;
		attachment.InstanceId = instanceId;
		CharacterComponentRuntime runtime = definition.CreateRuntime();
		if (runtime == null || runtime.Lifecycle != ComponentRuntimeLifecycle.Created || _registrationByRuntime.ContainsKey(runtime))
		{
			throw new InvalidOperationException("The definition must create a new runtime.");
		}
		attachment.Runtime = runtime;
		if (TryGetRuntimeByInstanceId(instanceId, out var _) || Owner != attachment.Owner || _isExitingTree || !RegisterRuntimeWireSlot(runtime, definition, text, attachment))
		{
			throw new InvalidOperationException("The component identity is occupied or the owner changed.");
		}
		_resourceComponents.Add(runtime);
		RegisterOwnerGameplayActivationRuntime(runtime, definition);
		_runtimeByInstanceId[instanceId] = runtime;
		if (_pendingNetworkStateByWireKey != null && _pendingNetworkStateByWireKey.TryGetValue(attachment.WireKey, out var value))
		{
			attachment.PendingState = value.Duplicate(deep: true);
			_pendingNetworkStateByWireKey.Remove(attachment.WireKey);
		}
		runtime.Bind(this, attachment.Owner, definition);
		RequireManaged();
		if (attachment.PendingState != null)
		{
			runtime.ApplyAuthoritativeSync(attachment.PendingState);
			RequireManaged();
		}
		if (IsOwnerInsideTree)
		{
			runtime.Activate();
			RequireManaged();
			runtime.RegisterStateRuntime();
			RequireManaged();
			RefreshRuntimePhysicsRegistration(runtime, refreshOwner: false);
			RefreshRuntimeInputRegistration(runtime, refreshOwner: false);
			RefreshRuntimeStateMachineRegistration(runtime, refreshOwner: false);
			RefreshOwnerProcessMembership();
		}
		attachment.Owner.RefreshResourceComponentFacades();
		RequireManaged();
		void RequireManaged()
		{
			if (!IsRuntimeManaged(runtime, instanceId))
			{
				throw new InvalidOperationException("The component removed itself or lost its owner during attachment.");
			}
		}
	}

	private void CaptureAttachmentNetworkState(string wireKey, Dictionary data)
	{
		if (_attachmentTransaction == null)
		{
			return;
		}
		RuntimeAttachment runtimeAttachment = _attachments.Find((RuntimeAttachment item) => item.WireKey == wireKey);
		if (runtimeAttachment == null)
		{
			return;
		}
		Dictionary dictionary = data.Duplicate(deep: true);
		if (!dictionary.ContainsKey("sm"))
		{
			Dictionary pendingState = runtimeAttachment.PendingState;
			if (pendingState != null && pendingState.ContainsKey("sm"))
			{
				dictionary["sm"] = runtimeAttachment.PendingState["sm"];
			}
		}
		runtimeAttachment.PendingState = dictionary;
	}

	private void RestoreAttachmentPendingState(RuntimeAttachment attachment)
	{
		if (attachment.PendingState == null || Owner != attachment.Owner)
		{
			return;
		}
		if (_pendingNetworkStateByWireKey == null)
		{
			_pendingNetworkStateByWireKey = new System.Collections.Generic.Dictionary<string, Dictionary>(StringComparer.Ordinal);
		}
		if (_pendingNetworkStateByWireKey.TryGetValue(attachment.WireKey, out var value))
		{
			if (!value.ContainsKey("sm") && attachment.PendingState.ContainsKey("sm"))
			{
				value["sm"] = attachment.PendingState["sm"];
			}
		}
		else
		{
			_pendingNetworkStateByWireKey[attachment.WireKey] = attachment.PendingState;
		}
	}

	private bool RestoreAttachmentIdentity(RuntimeAttachment attachment)
	{
		ComponentRegistration registration = attachment.Registration;
		if (registration == null)
		{
			return true;
		}
		if (!_slotsByType.TryGetValue(attachment.TypeName, out var value))
		{
			return true;
		}
		if (registration.TypeIndex >= value.Count || value[registration.TypeIndex] != registration)
		{
			return false;
		}
		if (registration.IdentityRevision != attachment.ReservedRevision || TryGetActiveComponent(registration, out var component) || TryGetActiveRuntime(registration, out var runtime))
		{
			return false;
		}
		if (registration.InstanceId != attachment.PreviousInstanceId || registration.DefinitionId != attachment.PreviousDefinitionId || registration.WireKey != attachment.PreviousWireKey)
		{
			if (!Global.IsMultiplayerMode)
			{
				System.Collections.Generic.Dictionary<string, Dictionary> pendingNetworkStateByWireKey = _pendingNetworkStateByWireKey;
				if (pendingNetworkStateByWireKey == null || !pendingNetworkStateByWireKey.ContainsKey(registration.WireKey))
				{
					goto IL_00c5;
				}
			}
			return false;
		}
		goto IL_00c5;
		IL_00c5:
		if (attachment.AppendedSlots.Count > 0)
		{
			if (value.Count != attachment.PreviousSlotCount + attachment.AppendedSlots.Count)
			{
				return false;
			}
			int num = 0;
			while (num < attachment.AppendedSlots.Count)
			{
				(ComponentRegistration, long) tuple = attachment.AppendedSlots[num];
				if (value[attachment.PreviousSlotCount + num] == tuple.Item1 && tuple.Item1.IdentityRevision == tuple.Item2 && !TryGetActiveComponent(tuple.Item1, out component) && !TryGetActiveRuntime(tuple.Item1, out runtime))
				{
					System.Collections.Generic.Dictionary<string, Dictionary> pendingNetworkStateByWireKey2 = _pendingNetworkStateByWireKey;
					if (pendingNetworkStateByWireKey2 == null || !pendingNetworkStateByWireKey2.ContainsKey(tuple.Item1.WireKey))
					{
						num++;
						continue;
					}
				}
				return false;
			}
			value.RemoveRange(attachment.PreviousSlotCount, attachment.AppendedSlots.Count);
			if (value.Count == 0)
			{
				_slotsByType.Remove(attachment.TypeName);
			}
		}
		else
		{
			registration.InstanceId = attachment.PreviousInstanceId;
			registration.DefinitionId = attachment.PreviousDefinitionId;
			registration.WireKey = attachment.PreviousWireKey;
			registration.IdentityRevision = attachment.PreviousRevision;
		}
		return true;
	}

	public ComponentManager()
	{
		base.ResourceLocalToScene = true;
	}

	public bool IsInsideTree()
	{
		return IsOwnerInsideTree;
	}

	public Node GetParent()
	{
		return parent;
	}

	public void AddChild(Node child)
	{
		if (GodotObject.IsInstanceValid(Owner) && GodotObject.IsInstanceValid(child))
		{
			Owner.AddChild(child, forceReadableName: false, Node.InternalMode.Disabled);
		}
	}

	public void RemoveChild(Node child)
	{
		if (GodotObject.IsInstanceValid(Owner) && GodotObject.IsInstanceValid(child) && child.GetParent() == Owner)
		{
			Owner.RemoveChild(child);
		}
	}

	public void AttachOwner(TowerDefenseCharacter owner)
	{
		if (!GodotObject.IsInstanceValid(owner))
		{
			return;
		}
		if (_ownerAttached && parent == owner)
		{
			RegisterExistingChildren();
			RefreshInputProcessMode();
			return;
		}
		if (_ownerAttached)
		{
			DetachOwner();
		}
		parent = owner;
		_ownerAttached = true;
		_isExitingTree = false;
		owner.SetProcessInput(enable: false);
		owner.SetProcessUnhandledInput(enable: false);
		owner.ChildEnteredTree += OnChildEnteredTree;
		owner.ChildExitingTree += OnChildExitingTree;
		RegisterExistingChildren();
		if (_resourceComponents.Count > 0)
		{
			RebindResourceComponents(owner);
			Callable.From(ActivateResourceComponents).CallDeferred();
		}
		RefreshInputProcessMode();
	}

	public void DetachOwner(ComponentDetachReason reason = ComponentDetachReason.TemporaryTreeExit)
	{
		CancelAttachmentTransaction();
		TowerDefenseCharacter owner = Owner;
		if (GodotObject.IsInstanceValid(owner))
		{
			owner.SetProcessInput(enable: false);
			owner.SetProcessUnhandledInput(enable: false);
			owner.ChildEnteredTree -= OnChildEnteredTree;
			owner.ChildExitingTree -= OnChildExitingTree;
		}
		_isExitingTree = true;
		DetachResourceComponents(reason);
		while (componentList.Count > 0)
		{
			List<ComponentBase> list = componentList;
			UnregisterComponent(list[list.Count - 1]);
		}
		_statefulComponents.Clear();
		_statefulIndices.Clear();
		_statefulRuntimes.Clear();
		_statefulRuntimeIndices.Clear();
		_physicsStatefulComponents?.Clear();
		_physicsStatefulRuntimes?.Clear();
		_physicsRuntimes.Clear();
		_physicsRuntimeIndices.Clear();
		_componentStatePhysicsMetricNames?.Clear();
		_runtimeStatePhysicsMetricNames?.Clear();
		_runtimePhysicsMetricNames?.Clear();
		_processStateMachineCount = 0;
		_physicsStateMachineCount = 0;
		_inputRuntimeCount = 0;
		_unhandledInputRuntimeCount = 0;
		componentList.Clear();
		_componentSet.Clear();
		componentDictionary.Clear();
		_pendingNetworkStateByWireKey?.Clear();
		_resourceStateRegistrationScheduled = false;
		_ownerAttached = false;
		if (GodotObject.IsInstanceValid(owner))
		{
			owner.RefreshComponentStateMachineDispatch();
		}
		parent = null;
		_isExitingTree = false;
	}

	public void Release()
	{
		CancelAttachmentTransaction();
		if (_ownerAttached || parent != null)
		{
			DetachOwner(ComponentDetachReason.OwnerReleased);
		}
		ReleaseResourceComponents(ComponentDetachReason.OwnerReleased);
		_registrationByComponent.Clear();
		_registrationByRuntime.Clear();
		_runtimeByInstanceId.Clear();
		_slotsByType.Clear();
		_pendingNetworkStateByWireKey?.Clear();
		_timerService?.Clear();
		_physicsStatefulComponents?.Clear();
		_physicsStatefulRuntimes?.Clear();
		_physicsRuntimes.Clear();
		_physicsRuntimeIndices.Clear();
		_componentStatePhysicsMetricNames?.Clear();
		_runtimeStatePhysicsMetricNames?.Clear();
		_runtimePhysicsMetricNames?.Clear();
		_processStateMachineCount = 0;
		_physicsStateMachineCount = 0;
		_inputRuntimeCount = 0;
		_unhandledInputRuntimeCount = 0;
	}

	public override void _Notification(int what)
	{
		if ((long)what == 1)
		{
			Release();
		}
	}

	public void InitializeResourceComponents()
	{
		CharacterComponentSet componentSet = ComponentSet;
		if (Engine.IsEditorHint() || !(parent is TowerDefenseCharacter towerDefenseCharacter))
		{
			return;
		}
		CancelAttachmentTransaction();
		CharacterComponentCreationPlan characterComponentCreationPlan = componentSet?.GetCreationPlan();
		if (_resourceComponentSet == componentSet && _appliedCreationPlan == characterComponentCreationPlan && _resourceComponents.Count > 0)
		{
			RebindResourceComponents(towerDefenseCharacter);
			return;
		}
		if (_resourceComponents.Count > 0)
		{
			ReleaseResourceComponents(ComponentDetachReason.DefinitionReplaced);
		}
		_resourceComponentSet = componentSet;
		_appliedCreationPlan = characterComponentCreationPlan;
		if (componentSet == null)
		{
			towerDefenseCharacter.RefreshResourceComponentFacades();
			return;
		}
		_resourceComponents.EnsureCapacity(_resourceComponents.Count + characterComponentCreationPlan.RuntimeCapacityHint);
		_runtimeByInstanceId.EnsureCapacity(_runtimeByInstanceId.Count + characterComponentCreationPlan.InstanceIdCapacityHint);
		_registrationByRuntime.EnsureCapacity(_registrationByRuntime.Count + characterComponentCreationPlan.RuntimeCapacityHint);
		_slotsByType.EnsureCapacity(_slotsByType.Count + characterComponentCreationPlan.ComponentTypeCapacityHint);
		for (int i = 0; i < characterComponentCreationPlan.Count; i++)
		{
			CharacterComponentCreationPlan.Entry entry = characterComponentCreationPlan[i];
			CharacterComponentRuntime characterComponentRuntime = null;
			try
			{
				characterComponentRuntime = entry.Definition.CreateRuntime();
				if (characterComponentRuntime == null)
				{
					GD.PushError("Character component definition '" + entry.Definition.ResourcePath + "' returned no runtime.");
					continue;
				}
				if (!RegisterRuntimeWireSlot(characterComponentRuntime, entry.Definition, entry.ResolvedComponentTypeId, entry.InstanceId, entry.ResolvedWireIndex, entry.WireKey, entry.WireSlotCapacityHint))
				{
					characterComponentRuntime.Release();
					continue;
				}
				_resourceComponents.Add(characterComponentRuntime);
				RegisterOwnerGameplayActivationRuntime(characterComponentRuntime, entry.Definition);
				_runtimeByInstanceId[entry.InstanceId] = characterComponentRuntime;
				characterComponentRuntime.Bind(this, towerDefenseCharacter, entry.Definition);
				if (_pendingNetworkStateByWireKey != null && _pendingNetworkStateByWireKey.Remove(entry.WireKey, out var value))
				{
					characterComponentRuntime.ApplyAuthoritativeSync(value);
				}
			}
			catch (Exception exception)
			{
				RollbackFailedRuntime(characterComponentRuntime, exception);
			}
		}
		towerDefenseCharacter.RefreshResourceComponentFacades();
	}

	public void ActivateResourceComponents()
	{
		if (!IsOwnerInsideTree)
		{
			return;
		}
		for (int i = 0; i < _resourceComponents.Count; i++)
		{
			CharacterComponentRuntime characterComponentRuntime = _resourceComponents[i];
			try
			{
				characterComponentRuntime.Activate();
				RefreshRuntimePhysicsRegistration(characterComponentRuntime, refreshOwner: false);
				RefreshRuntimeInputRegistration(characterComponentRuntime, refreshOwner: false);
			}
			catch (Exception exception)
			{
				RollbackFailedRuntime(characterComponentRuntime, exception);
				i--;
			}
		}
		RefreshOwnerProcessMembership();
		ScheduleResourceStateRegistration();
	}

	internal void NotifyOwnerGameplayActivated()
	{
		CharacterComponentRuntime singleOwnerGameplayActivationRuntime = _singleOwnerGameplayActivationRuntime;
		if (singleOwnerGameplayActivationRuntime != null)
		{
			singleOwnerGameplayActivationRuntime.OwnerGameplayActivated();
			if (_ownerGameplayActivationRuntimes.Count > 1)
			{
				for (int i = 1; i < _ownerGameplayActivationRuntimes.Count; i++)
				{
					_ownerGameplayActivationRuntimes[i].OwnerGameplayActivated();
				}
			}
		}
		else
		{
			for (int j = 0; j < _ownerGameplayActivationRuntimes.Count; j++)
			{
				_ownerGameplayActivationRuntimes[j].OwnerGameplayActivated();
			}
		}
	}

	internal void NotifyOwnerBeforeDestroy()
	{
		for (int i = 0; i < _resourceComponents.Count; i++)
		{
			_resourceComponents[i].OwnerBeforeDestroy();
		}
	}

	private void ScheduleResourceStateRegistration()
	{
		if (!_resourceStateRegistrationScheduled && IsOwnerInsideTree)
		{
			_resourceStateRegistrationScheduled = true;
			Callable.From(RegisterResourceStateRuntimes).CallDeferred();
		}
	}

	private void RegisterResourceStateRuntimes()
	{
		_resourceStateRegistrationScheduled = false;
		if (!IsOwnerInsideTree)
		{
			return;
		}
		for (int i = 0; i < _resourceComponents.Count; i++)
		{
			CharacterComponentRuntime characterComponentRuntime = _resourceComponents[i];
			try
			{
				characterComponentRuntime.RegisterStateRuntime();
				RefreshRuntimeStateMachineRegistration(characterComponentRuntime, refreshOwner: false);
			}
			catch (Exception exception)
			{
				RollbackFailedRuntime(characterComponentRuntime, exception);
				i--;
			}
		}
		RefreshOwnerProcessMembership();
	}

	public T GetRuntime<T>() where T : CharacterComponentRuntime
	{
		for (int i = 0; i < _resourceComponents.Count; i++)
		{
			if (_resourceComponents[i] is T val && !val.IsReleased)
			{
				return val;
			}
		}
		return null;
	}

	public T GetRuntime<T>(string instanceId) where T : CharacterComponentRuntime
	{
		if (string.IsNullOrWhiteSpace(instanceId) || !_runtimeByInstanceId.TryGetValue(instanceId, out var value) || value.IsReleased)
		{
			return null;
		}
		return value as T;
	}

	public T GetComponent<T>() where T : class
	{
		for (int i = 0; i < componentList.Count; i++)
		{
			if (componentList[i] is T result && GodotObject.IsInstanceValid(componentList[i]))
			{
				return result;
			}
		}
		for (int j = 0; j < _resourceComponents.Count; j++)
		{
			if (_resourceComponents[j] is T result2 && !_resourceComponents[j].IsReleased)
			{
				return result2;
			}
		}
		return null;
	}

	public CharacterComponentRuntime AddRuntimeComponent(CharacterComponentDefinition definition)
	{
		if (definition == null || !(parent is TowerDefenseCharacter owner))
		{
			return null;
		}
		string value = ResolveLegacyComponentName(definition.ComponentTypeId?.Trim());
		string text = definition.InstanceId?.Trim();
		if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(text))
		{
			GD.PushError("Dynamic character component definition requires stable ComponentTypeId and InstanceId values.");
			return null;
		}
		if (_runtimeByInstanceId.TryGetValue(text, out var value2) && value2 != null && !value2.IsReleased)
		{
			return value2;
		}
		RuntimeAttachment runtimeAttachment = new RuntimeAttachment
		{
			InstanceId = text,
			Owner = owner
		};
		try
		{
			AttachRuntimeComponentCore(definition, runtimeAttachment);
			return runtimeAttachment.Runtime;
		}
		catch (Exception exception)
		{
			RollbackFailedRuntime(runtimeAttachment.Runtime, exception);
			RestoreAttachmentPendingState(runtimeAttachment);
			return null;
		}
	}

	private void RollbackFailedRuntime(CharacterComponentRuntime runtime, Exception exception)
	{
		if (runtime != null && !RemoveRuntimeComponent(runtime))
		{
			runtime.Release(ComponentDetachReason.DefinitionReplaced);
			DeactivateRuntimeWireSlot(runtime);
		}
		GD.PushWarning("[ComponentManager] 组件创建或生命周期失败，已回滚：" + exception.GetBaseException().Message);
	}

	public bool RemoveRuntimeComponent(CharacterComponentRuntime runtime)
	{
		if (runtime == null || !_resourceComponents.Remove(runtime))
		{
			return false;
		}
		UnregisterOwnerGameplayActivationRuntime(runtime);
		string[] array = (from pair in _runtimeByInstanceId
			where pair.Value == runtime
			select pair.Key).ToArray();
		foreach (string key in array)
		{
			_runtimeByInstanceId.Remove(key);
		}
		RemoveStatefulRuntime(runtime);
		RemovePhysicsRuntime(runtime);
		ClearRuntimeInputWorkRegistration(runtime);
		try
		{
			runtime.Release(ComponentDetachReason.DefinitionReplaced);
		}
		finally
		{
			DeactivateRuntimeWireSlot(runtime);
		}
		if (parent is TowerDefenseCharacter towerDefenseCharacter)
		{
			towerDefenseCharacter.RefreshResourceComponentFacades();
		}
		RefreshOwnerProcessMembership();
		return true;
	}

	private void RegisterOwnerGameplayActivationRuntime(CharacterComponentRuntime runtime, CharacterComponentDefinition definition)
	{
		if (ShouldRegisterOwnerGameplayActivationRuntime(runtime, definition))
		{
			_ownerGameplayActivationRuntimes.Add(runtime);
			_singleOwnerGameplayActivationRuntime = ((_ownerGameplayActivationRuntimes.Count == 1) ? runtime : null);
		}
	}

	private static bool ShouldRegisterOwnerGameplayActivationRuntime(CharacterComponentRuntime runtime, CharacterComponentDefinition definition)
	{
		if (runtime == null || definition == null)
		{
			return false;
		}
		if (runtime.HasOwnerGameplayActivationWork || definition is ModCharacterComponentDefinition)
		{
			return true;
		}
		if (!(runtime.GetType().Assembly != typeof(CharacterComponentRuntime).Assembly))
		{
			return definition.GetType().Assembly != typeof(CharacterComponentDefinition).Assembly;
		}
		return true;
	}

	private void UnregisterOwnerGameplayActivationRuntime(CharacterComponentRuntime runtime)
	{
		if (runtime != null && _ownerGameplayActivationRuntimes.Remove(runtime))
		{
			if (_ownerGameplayActivationRuntimes.Count == 1)
			{
				_singleOwnerGameplayActivationRuntime = _ownerGameplayActivationRuntimes[0];
			}
			else
			{
				_singleOwnerGameplayActivationRuntime = null;
			}
		}
	}

	private void RebindResourceComponents(TowerDefenseCharacter owner)
	{
		for (int i = 0; i < _resourceComponents.Count; i++)
		{
			CharacterComponentRuntime characterComponentRuntime = _resourceComponents[i];
			try
			{
				if (!characterComponentRuntime.IsReleased)
				{
					characterComponentRuntime.Bind(this, owner, characterComponentRuntime.ComponentDefinition);
				}
			}
			catch (Exception exception)
			{
				RollbackFailedRuntime(characterComponentRuntime, exception);
				i--;
			}
		}
	}

	private void DetachResourceComponents(ComponentDetachReason reason)
	{
		for (int i = 0; i < _resourceComponents.Count; i++)
		{
			CharacterComponentRuntime characterComponentRuntime = _resourceComponents[i];
			RemoveStatefulRuntime(characterComponentRuntime);
			RemovePhysicsRuntime(characterComponentRuntime);
			ClearRuntimeInputWorkRegistration(characterComponentRuntime);
			try
			{
				characterComponentRuntime.Detach(reason);
			}
			catch (Exception exception)
			{
				RollbackFailedRuntime(characterComponentRuntime, exception);
				i--;
			}
		}
		RefreshOwnerProcessMembership();
	}

	private void ReleaseResourceComponents(ComponentDetachReason reason)
	{
		for (int num = _resourceComponents.Count - 1; num >= 0; num--)
		{
			CharacterComponentRuntime characterComponentRuntime = _resourceComponents[num];
			UnregisterOwnerGameplayActivationRuntime(characterComponentRuntime);
			RemoveStatefulRuntime(characterComponentRuntime);
			RemovePhysicsRuntime(characterComponentRuntime);
			ClearRuntimeInputWorkRegistration(characterComponentRuntime);
			characterComponentRuntime.Release(reason);
			DeactivateRuntimeWireSlot(characterComponentRuntime);
		}
		_resourceComponents.Clear();
		_ownerGameplayActivationRuntimes.Clear();
		_singleOwnerGameplayActivationRuntime = null;
		_runtimeByInstanceId.Clear();
		_resourceComponentSet = null;
		_appliedCreationPlan = null;
	}

	private void RegisterExistingChildren()
	{
		TowerDefenseCharacter owner = Owner;
		if (!GodotObject.IsInstanceValid(owner) || !owner.IsInsideTree())
		{
			return;
		}
		foreach (Node child in owner.GetChildren())
		{
			if (child is ComponentBase component)
			{
				RegisterComponent(component);
			}
		}
	}

	private void OnChildEnteredTree(Node child)
	{
		if (child is ComponentBase component)
		{
			RegisterComponent(component);
		}
	}

	private void OnChildExitingTree(Node child)
	{
		if (child is ComponentBase component)
		{
			UnregisterComponent(component);
		}
	}

	public void RegisterComponent(ComponentBase component)
	{
		TowerDefenseCharacter owner = Owner;
		if (GodotObject.IsInstanceValid(component) && GodotObject.IsInstanceValid(owner) && component.IsInsideTree() && component.GetParent() == owner && _componentSet.Add(component))
		{
			componentList.Add(component);
			string componentTypeName = GetComponentTypeName(component);
			if (!componentDictionary.TryGetValue(componentTypeName, out var value))
			{
				value = new List<ComponentBase>();
				componentDictionary[componentTypeName] = value;
			}
			value.Add(component);
			RegisterWireSlot(component, componentTypeName);
			Callable.From(delegate
			{
				AttachRegisteredComponent(component);
			}).CallDeferred();
		}
	}

	public void UnregisterComponent(ComponentBase component)
	{
		if (component == null || !_componentSet.Remove(component))
		{
			return;
		}
		RemoveStatefulComponent(component);
		if (TryGetWireKey(component, out var wireKey))
		{
			if (component.IsQueuedForDeletion())
			{
				_timerService?.RemoveComponent(component, wireKey);
			}
			else
			{
				_timerService?.DetachComponent(component, wireKey);
			}
		}
		DeactivateWireSlot(component);
		component.DetachFromManager(this);
		componentList.Remove(component);
		string componentTypeName = GetComponentTypeName(component);
		if (componentDictionary.TryGetValue(componentTypeName, out var value))
		{
			value.Remove(component);
			if (value.Count == 0)
			{
				componentDictionary.Remove(componentTypeName);
			}
		}
		RefreshOwnerProcessMembership();
	}

	private void AttachRegisteredComponent(ComponentBase component)
	{
		TowerDefenseCharacter owner = Owner;
		if (IsOwnerInsideTree && GodotObject.IsInstanceValid(component) && _componentSet.Contains(component) && component.GetParent() == owner)
		{
			component.AttachToManager(this);
			if (TryGetWireKey(component, out var wireKey))
			{
				_timerService?.AttachComponent(component, wireKey);
			}
			if (_pendingNetworkStateByWireKey != null && TryGetWireKey(component, out var wireKey2) && _pendingNetworkStateByWireKey.Remove(wireKey2, out var value))
			{
				component.ApplyAuthoritativeSync(value);
			}
		}
	}

	internal void ApplyOrQueueNetworkState(string wireKey, Dictionary data)
	{
		if (string.IsNullOrWhiteSpace(wireKey) || data == null)
		{
			return;
		}
		CaptureAttachmentNetworkState(wireKey, data);
		if (TryGetComponentByWireKey(wireKey, out var component))
		{
			component.ApplyAuthoritativeSync(data);
			return;
		}
		if (TryGetRuntimeByWireKey(wireKey, out var runtime))
		{
			runtime.ApplyAuthoritativeSync(data);
			return;
		}
		Dictionary dictionary = data.Duplicate(deep: true);
		if (!dictionary.ContainsKey("sm") && _pendingNetworkStateByWireKey != null && _pendingNetworkStateByWireKey.TryGetValue(wireKey, out var value) && value.ContainsKey("sm"))
		{
			dictionary["sm"] = value["sm"];
		}
		if (_pendingNetworkStateByWireKey == null)
		{
			_pendingNetworkStateByWireKey = new System.Collections.Generic.Dictionary<string, Dictionary>(StringComparer.Ordinal);
		}
		_pendingNetworkStateByWireKey[wireKey] = dictionary;
	}

	internal void RefreshStateMachineRegistration(ComponentBase component)
	{
		if (!GodotObject.IsInstanceValid(component) || !_componentSet.Contains(component) || !component.HasStateMachine)
		{
			RemoveStatefulComponent(component);
		}
		else if (_statefulIndices.TryAdd(component, _statefulComponents.Count))
		{
			_statefulComponents.Add(component);
		}
		RefreshStateMachineWorkRegistration(component);
		RefreshOwnerProcessMembership();
	}

	internal void NotifyComponentAliveChanged(ComponentBase component)
	{
		if (component != null && _componentSet.Contains(component))
		{
			RefreshStateMachineWorkRegistration(component);
			RefreshOwnerProcessMembership();
		}
	}

	internal void NotifyRuntimeAliveChanged(CharacterComponentRuntime runtime)
	{
		if (runtime != null && _registrationByRuntime.ContainsKey(runtime))
		{
			RefreshRuntimeStateMachineRegistration(runtime, refreshOwner: false);
			RefreshRuntimePhysicsRegistration(runtime, refreshOwner: false);
			RefreshRuntimeInputRegistration(runtime, refreshOwner: false);
			RefreshOwnerProcessMembership();
		}
	}

	internal void DispatchRuntimeInput(InputEvent inputEvent)
	{
		if (inputEvent == null || _inputRuntimeCount <= 0)
		{
			return;
		}
		for (int i = 0; i < _resourceComponents.Count; i++)
		{
			CharacterComponentRuntime characterComponentRuntime = _resourceComponents[i];
			if (characterComponentRuntime != null && characterComponentRuntime.HasRuntimeInputWork && characterComponentRuntime.CanDispatchInputWork)
			{
				characterComponentRuntime.ProcessInput(inputEvent);
			}
		}
	}

	public void _Input(InputEvent inputEvent)
	{
		DispatchRuntimeInput(inputEvent);
	}

	internal void DispatchRuntimeUnhandledInput(InputEvent inputEvent)
	{
		if (inputEvent == null || _unhandledInputRuntimeCount <= 0)
		{
			return;
		}
		for (int i = 0; i < _resourceComponents.Count; i++)
		{
			CharacterComponentRuntime characterComponentRuntime = _resourceComponents[i];
			if (characterComponentRuntime != null && characterComponentRuntime.HasRuntimeUnhandledInputWork && characterComponentRuntime.CanDispatchInputWork)
			{
				characterComponentRuntime.ProcessUnhandledInput(inputEvent);
			}
		}
	}

	public void _UnhandledInput(InputEvent inputEvent)
	{
		DispatchRuntimeUnhandledInput(inputEvent);
	}

	internal void TickStateMachineProcess(double delta)
	{
		int num = 0;
		while (num < _statefulComponents.Count)
		{
			ComponentBase componentBase = _statefulComponents[num];
			if (!GodotObject.IsInstanceValid(componentBase) || !componentBase.HasStateMachine)
			{
				RemoveStatefulAtSwap(num);
				continue;
			}
			if (componentBase.HasStateMachineProcessWork && componentBase.CanDispatchStateMachineWork)
			{
				componentBase.StateMachine.TickProcess(delta);
			}
			if (num < _statefulComponents.Count && _statefulComponents[num] == componentBase)
			{
				num++;
			}
		}
		int num2 = 0;
		while (num2 < _statefulRuntimes.Count)
		{
			CharacterComponentRuntime characterComponentRuntime = _statefulRuntimes[num2];
			if (characterComponentRuntime.IsReleased || !characterComponentRuntime.HasStateMachine)
			{
				RemoveStatefulRuntimeAtSwap(num2);
				continue;
			}
			if (characterComponentRuntime.HasStateMachineProcessWork && characterComponentRuntime.CanDispatchStateMachineWork)
			{
				characterComponentRuntime.StateMachine.TickProcess(delta);
			}
			if (num2 < _statefulRuntimes.Count && _statefulRuntimes[num2] == characterComponentRuntime)
			{
				num2++;
			}
		}
	}

	internal void TickStateMachinePhysics(double delta, bool ownerInsideComponentBattlefield)
	{
		bool flag = TowerDefensePerfProfiler.Enabled && TowerDefensePerfProfiler.DetailedHotPathMetrics;
		int num = 0;
		while (num < (_physicsStatefulComponents?.Count ?? 0))
		{
			ComponentBase componentBase = _physicsStatefulComponents[num];
			if (!GodotObject.IsInstanceValid(componentBase) || !componentBase.HasStateMachine)
			{
				RemoveStatefulComponent(componentBase);
				continue;
			}
			if (!componentBase.HasStateMachinePhysicsWork)
			{
				RefreshStateMachineWorkRegistration(componentBase);
				continue;
			}
			if (componentBase.CanDispatchStateMachineWork)
			{
				if (flag)
				{
					string componentStatePhysicsMetricName = GetComponentStatePhysicsMetricName(componentBase);
					long startTicks = TowerDefensePerfProfiler.BeginHotPath();
					componentBase.StateMachine.TickPhysics(delta);
					TowerDefensePerfProfiler.End(componentStatePhysicsMetricName, startTicks);
				}
				else
				{
					componentBase.StateMachine.TickPhysics(delta);
				}
			}
			if (num < _physicsStatefulComponents.Count && _physicsStatefulComponents[num] == componentBase)
			{
				num++;
			}
		}
		int num2 = 0;
		while (num2 < (_physicsStatefulRuntimes?.Count ?? 0))
		{
			CharacterComponentRuntime characterComponentRuntime = _physicsStatefulRuntimes[num2];
			if (characterComponentRuntime.IsReleased || !characterComponentRuntime.HasStateMachine)
			{
				RemoveStatefulRuntime(characterComponentRuntime);
				continue;
			}
			if (!characterComponentRuntime.HasStateMachinePhysicsWork)
			{
				RefreshRuntimeStateMachineWorkRegistration(characterComponentRuntime);
				continue;
			}
			if (characterComponentRuntime.CanDispatchStateMachineWorkForOwnerState(ownerInsideComponentBattlefield))
			{
				if (flag)
				{
					string runtimeStatePhysicsMetricName = GetRuntimeStatePhysicsMetricName(characterComponentRuntime);
					long startTicks2 = TowerDefensePerfProfiler.BeginHotPath();
					TickRuntimeStateMachinePhysics(characterComponentRuntime, delta);
					TowerDefensePerfProfiler.End(runtimeStatePhysicsMetricName, startTicks2);
				}
				else
				{
					TickRuntimeStateMachinePhysics(characterComponentRuntime, delta);
				}
			}
			if (num2 < _physicsStatefulRuntimes.Count && _physicsStatefulRuntimes[num2] == characterComponentRuntime)
			{
				num2++;
			}
		}
	}

	private static void TickRuntimeStateMachinePhysics(CharacterComponentRuntime runtime, double delta)
	{
		if (!runtime.TryTickFlatStateMachinePhysics(delta))
		{
			runtime.StateMachine.TickPhysics(delta);
		}
	}

	private string GetComponentStatePhysicsMetricName(ComponentBase component)
	{
		Type type = component.GetType();
		if (_componentStatePhysicsMetricNames != null && _componentStatePhysicsMetricNames.TryGetValue(type, out var value))
		{
			return value;
		}
		value = "batch.component.state." + type.Name;
		if (_componentStatePhysicsMetricNames == null)
		{
			_componentStatePhysicsMetricNames = new System.Collections.Generic.Dictionary<Type, string>();
		}
		_componentStatePhysicsMetricNames.Add(type, value);
		return value;
	}

	private string GetRuntimeStatePhysicsMetricName(CharacterComponentRuntime runtime)
	{
		Type type = runtime.GetType();
		if (_runtimeStatePhysicsMetricNames != null && _runtimeStatePhysicsMetricNames.TryGetValue(type, out var value))
		{
			return value;
		}
		value = "batch.component.runtimeState." + type.Name;
		if (_runtimeStatePhysicsMetricNames == null)
		{
			_runtimeStatePhysicsMetricNames = new System.Collections.Generic.Dictionary<Type, string>();
		}
		_runtimeStatePhysicsMetricNames.Add(type, value);
		return value;
	}

	internal void TickRuntimePhysics(double delta, ulong physicsFrame, bool ownerInsideComponentBattlefield)
	{
		bool flag = TowerDefensePerfProfiler.Enabled && TowerDefensePerfProfiler.DetailedHotPathMetrics;
		int num = 0;
		while (num < _physicsRuntimes.Count)
		{
			CharacterComponentRuntime characterComponentRuntime = _physicsRuntimes[num];
			if (characterComponentRuntime == null || !characterComponentRuntime.HasRuntimePhysicsWork)
			{
				RemovePhysicsRuntimeAtSwap(num);
				continue;
			}
			if (characterComponentRuntime.CanDispatchPhysicsWorkForOwnerState(ownerInsideComponentBattlefield))
			{
				if (flag)
				{
					long startTicks = TowerDefensePerfProfiler.BeginHotPath();
					characterComponentRuntime.DispatchPhysicsProcess(delta, physicsFrame);
					TowerDefensePerfProfiler.End(GetRuntimePhysicsMetricName(characterComponentRuntime), startTicks);
				}
				else
				{
					characterComponentRuntime.DispatchPhysicsProcess(delta, physicsFrame);
				}
			}
			if (num < _physicsRuntimes.Count && _physicsRuntimes[num] == characterComponentRuntime)
			{
				num++;
			}
		}
	}

	private string GetRuntimePhysicsMetricName(CharacterComponentRuntime runtime)
	{
		Type type = runtime.GetType();
		if (_runtimePhysicsMetricNames != null && _runtimePhysicsMetricNames.TryGetValue(type, out var value))
		{
			return value;
		}
		value = "batch.component.runtime." + type.Name;
		if (_runtimePhysicsMetricNames == null)
		{
			_runtimePhysicsMetricNames = new System.Collections.Generic.Dictionary<Type, string>();
		}
		_runtimePhysicsMetricNames.Add(type, value);
		return value;
	}

	internal void TickComponentTimers(double delta, bool ownerInsideComponentBattlefield)
	{
		if (ownerInsideComponentBattlefield)
		{
			ComponentTimerService timerService = _timerService;
			if (timerService != null && timerService.HasActiveTimers && TowerDefenseManager._IsGameRunning())
			{
				double ownerTimeScale = ((parent is TowerDefenseCharacter towerDefenseCharacter) ? towerDefenseCharacter.GetLogicTimeScale() : 1.0);
				_timerService.Tick(delta, ownerTimeScale);
			}
		}
	}

	internal bool StartComponentTimer(ComponentBase component, StringName timerName, double duration, bool useOwnerTimeScale)
	{
		if (!TryGetComponentTimerKey(component, timerName, out var key))
		{
			return false;
		}
		if (_timerService == null)
		{
			_timerService = new ComponentTimerService();
		}
		return _timerService.Start(component, key, duration, useOwnerTimeScale);
	}

	internal bool StopComponentTimer(ComponentBase component, StringName timerName)
	{
		if (_timerService != null && TryGetComponentTimerKey(component, timerName, out var key))
		{
			return _timerService.Stop(component, key);
		}
		return false;
	}

	internal bool PauseComponentTimer(ComponentBase component, StringName timerName)
	{
		if (_timerService != null && TryGetComponentTimerKey(component, timerName, out var key))
		{
			return _timerService.Pause(component, key);
		}
		return false;
	}

	internal bool ResumeComponentTimer(ComponentBase component, StringName timerName)
	{
		if (_timerService != null && TryGetComponentTimerKey(component, timerName, out var key))
		{
			return _timerService.Resume(component, key);
		}
		return false;
	}

	internal bool IsComponentTimerRunning(ComponentBase component, StringName timerName)
	{
		if (_timerService != null && TryGetComponentTimerKey(component, timerName, out var key))
		{
			return _timerService.IsRunning(component, key);
		}
		return false;
	}

	internal double GetComponentTimerRemaining(ComponentBase component, StringName timerName)
	{
		if (_timerService == null || !TryGetComponentTimerKey(component, timerName, out var key))
		{
			return 0.0;
		}
		return _timerService.Remaining(component, key);
	}

	private bool TryGetComponentTimerKey(ComponentBase component, StringName timerName, out ComponentTimerKey key)
	{
		key = default(ComponentTimerKey);
		if (component == null || timerName.IsEmpty || !TryGetWireKey(component, out var wireKey))
		{
			return false;
		}
		key = new ComponentTimerKey(wireKey, timerName);
		return true;
	}

	internal bool TryGetWireKey(ComponentBase component, out string wireKey)
	{
		wireKey = string.Empty;
		if (component == null || !_registrationByComponent.TryGetValue(component, out var value) || !TryGetActiveComponent(value, out var component2) || component2 != component)
		{
			return false;
		}
		wireKey = value.WireKey;
		return true;
	}

	internal bool TryGetWireKey(CharacterComponentRuntime runtime, out string wireKey)
	{
		wireKey = string.Empty;
		if (runtime == null || runtime.IsReleased || !_registrationByRuntime.TryGetValue(runtime, out var value) || value.ActiveRuntime != runtime)
		{
			return false;
		}
		wireKey = value.WireKey;
		return true;
	}

	internal bool TryGetComponentByWireKey(string wireKey, out ComponentBase component)
	{
		component = null;
		if (!TryParseWireKey(wireKey, out var typeName, out var typeIndex) || !_slotsByType.TryGetValue(typeName, out var value) || typeIndex < 0 || typeIndex >= value.Count)
		{
			return false;
		}
		return TryGetActiveComponent(value[typeIndex], out component);
	}

	internal bool TryGetRuntimeByWireKey(string wireKey, out CharacterComponentRuntime runtime)
	{
		runtime = null;
		if (!TryParseWireKey(wireKey, out var typeName, out var typeIndex) || !_slotsByType.TryGetValue(typeName, out var value) || typeIndex < 0 || typeIndex >= value.Count)
		{
			return false;
		}
		return TryGetActiveRuntime(value[typeIndex], out runtime);
	}

	internal bool TryGetRuntimeByInstanceId(string instanceId, out CharacterComponentRuntime runtime)
	{
		if (!string.IsNullOrEmpty(instanceId) && _runtimeByInstanceId.TryGetValue(instanceId, out runtime) && !runtime.IsReleased)
		{
			return true;
		}
		runtime = null;
		return false;
	}

	internal bool TryGetRuntimeByLegacyNodeName(string nodeName, out CharacterComponentRuntime runtime)
	{
		if (!string.IsNullOrEmpty(nodeName))
		{
			for (int i = 0; i < _resourceComponents.Count; i++)
			{
				CharacterComponentRuntime characterComponentRuntime = _resourceComponents[i];
				if (characterComponentRuntime.IsReleased)
				{
					continue;
				}
				Array<StringName> array = characterComponentRuntime.ComponentDefinition?.LegacyNodeNames;
				if (array == null)
				{
					continue;
				}
				for (int j = 0; j < array.Count; j++)
				{
					if (string.Equals(array[j].ToString(), nodeName, StringComparison.Ordinal))
					{
						runtime = characterComponentRuntime;
						return true;
					}
				}
			}
		}
		runtime = null;
		return false;
	}

	private void RegisterWireSlot(ComponentBase component, string typeName)
	{
		if (!_registrationByComponent.ContainsKey(component))
		{
			if (!_slotsByType.TryGetValue(typeName, out var value))
			{
				value = new List<ComponentRegistration>();
				_slotsByType[typeName] = value;
			}
			ulong instanceId = GetInstanceId();
			ComponentRegistration componentRegistration = null;
			if (component.TryGetWireSlotToken(instanceId, typeName, out var typeIndex) && typeIndex >= 0 && typeIndex < value.Count && !TryGetActiveComponent(value[typeIndex], out var _) && !TryGetActiveRuntime(value[typeIndex], out var _))
			{
				componentRegistration = value[typeIndex];
			}
			if (componentRegistration == null)
			{
				componentRegistration = new ComponentRegistration
				{
					TypeName = typeName,
					TypeIndex = value.Count,
					WireKey = BuildWireKey(typeName, value.Count)
				};
				value.Add(componentRegistration);
			}
			componentRegistration.ActiveComponent = new WeakReference<ComponentBase>(component);
			componentRegistration.IdentityRevision++;
			_registrationByComponent[component] = componentRegistration;
			component.SetWireSlotToken(instanceId, typeName, componentRegistration.TypeIndex);
		}
	}

	private bool RegisterRuntimeWireSlot(CharacterComponentRuntime runtime, CharacterComponentDefinition definition, string typeName, RuntimeAttachment attachment = null)
	{
		string instanceId = definition?.InstanceId?.Trim();
		return RegisterRuntimeWireSlot(runtime, definition, typeName, instanceId, definition?.WireIndex ?? (-1), null, 1, attachment);
	}

	private bool RegisterRuntimeWireSlot(CharacterComponentRuntime runtime, CharacterComponentDefinition definition, string typeName, string instanceId, int wireIndex, string wireKey, int wireSlotCapacityHint, RuntimeAttachment attachment = null)
	{
		if (runtime == null || definition == null || _registrationByRuntime.ContainsKey(runtime))
		{
			return false;
		}
		if (!_slotsByType.TryGetValue(typeName, out var value))
		{
			value = new List<ComponentRegistration>(Math.Max(1, wireSlotCapacityHint));
			_slotsByType[typeName] = value;
		}
		else
		{
			value.EnsureCapacity(Math.Max(value.Count, wireSlotCapacityHint));
		}
		int count = value.Count;
		int num = wireIndex;
		ComponentBase component;
		CharacterComponentRuntime runtime2;
		if (num < 0)
		{
			for (int i = 0; i < value.Count; i++)
			{
				ComponentRegistration componentRegistration = value[i];
				if (string.Equals(componentRegistration.InstanceId, instanceId, StringComparison.Ordinal) && !TryGetActiveComponent(componentRegistration, out component) && !TryGetActiveRuntime(componentRegistration, out runtime2))
				{
					num = i;
					break;
				}
			}
			if (num < 0)
			{
				num = value.Count;
			}
		}
		while (value.Count <= num)
		{
			value.Add(new ComponentRegistration
			{
				TypeName = typeName,
				TypeIndex = value.Count,
				WireKey = BuildWireKey(typeName, value.Count)
			});
		}
		ComponentRegistration componentRegistration2 = value[num];
		if (!string.IsNullOrEmpty(componentRegistration2.InstanceId) && !string.Equals(componentRegistration2.InstanceId, instanceId, StringComparison.Ordinal))
		{
			GD.PushError($"Character component wire slot '{componentRegistration2.WireKey}' belongs to InstanceId '{componentRegistration2.InstanceId}', not '{instanceId}'.");
			return false;
		}
		if (TryGetActiveComponent(componentRegistration2, out component) || TryGetActiveRuntime(componentRegistration2, out runtime2))
		{
			GD.PushError("Character component wire slot '" + componentRegistration2.WireKey + "' is already occupied.");
			return false;
		}
		if (attachment != null)
		{
			attachment.TypeName = typeName;
			attachment.Registration = componentRegistration2;
			attachment.PreviousInstanceId = componentRegistration2.InstanceId;
			attachment.PreviousDefinitionId = componentRegistration2.DefinitionId;
			attachment.PreviousWireKey = componentRegistration2.WireKey;
			attachment.PreviousRevision = componentRegistration2.IdentityRevision;
			attachment.PreviousSlotCount = count;
		}
		componentRegistration2.ActiveRuntime = runtime;
		componentRegistration2.InstanceId = instanceId;
		componentRegistration2.DefinitionId = definition.DefinitionId;
		componentRegistration2.WireKey = (string.IsNullOrEmpty(wireKey) ? BuildWireKey(typeName, num) : wireKey);
		componentRegistration2.IdentityRevision++;
		_registrationByRuntime[runtime] = componentRegistration2;
		if (attachment != null)
		{
			attachment.WireKey = componentRegistration2.WireKey;
			attachment.ReservedRevision = componentRegistration2.IdentityRevision;
			attachment.AppendedSlots = new List<(ComponentRegistration, long)>();
			for (int j = count; j < value.Count; j++)
			{
				attachment.AppendedSlots.Add((value[j], value[j].IdentityRevision));
			}
		}
		return true;
	}

	private static string BuildWireKey(string typeName, int typeIndex)
	{
		if (typeIndex != 0)
		{
			return $"{typeName}#{typeIndex}";
		}
		return typeName;
	}

	private void DeactivateWireSlot(ComponentBase component)
	{
		if (_registrationByComponent.Remove(component, out var value) && TryGetActiveComponent(value, out var component2) && component2 == component)
		{
			value.ActiveComponent = null;
		}
	}

	private void DeactivateRuntimeWireSlot(CharacterComponentRuntime runtime)
	{
		if (runtime != null && _registrationByRuntime.Remove(runtime, out var value) && value.ActiveRuntime == runtime)
		{
			value.ActiveRuntime = null;
		}
	}

	private static bool TryGetActiveComponent(ComponentRegistration registration, out ComponentBase component)
	{
		component = null;
		if (registration?.ActiveComponent != null && registration.ActiveComponent.TryGetTarget(out component))
		{
			return GodotObject.IsInstanceValid(component);
		}
		return false;
	}

	private static bool TryGetActiveRuntime(ComponentRegistration registration, out CharacterComponentRuntime runtime)
	{
		runtime = registration?.ActiveRuntime;
		if (runtime != null)
		{
			return !runtime.IsReleased;
		}
		return false;
	}

	internal static bool TryParseWireKey(string wireKey, out string typeName, out int typeIndex)
	{
		typeName = string.Empty;
		typeIndex = 0;
		if (string.IsNullOrEmpty(wireKey))
		{
			return false;
		}
		int num = wireKey.LastIndexOf('#');
		if (num < 0)
		{
			typeName = ResolveLegacyComponentName(wireKey);
			return true;
		}
		if (num != 0 && num != wireKey.Length - 1)
		{
			int num2 = num + 1;
			if (int.TryParse(wireKey.Substring(num2, wireKey.Length - num2), out typeIndex) && typeIndex >= 0)
			{
				typeName = ResolveLegacyComponentName(wireKey.Substring(0, num));
				return true;
			}
		}
		return false;
	}

	internal void RefreshRuntimeStateMachineRegistration(CharacterComponentRuntime runtime)
	{
		RefreshRuntimeStateMachineRegistration(runtime, refreshOwner: true);
	}

	internal void RefreshRuntimePhysicsRegistration(CharacterComponentRuntime runtime)
	{
		RefreshRuntimePhysicsRegistration(runtime, refreshOwner: false);
	}

	private void RefreshRuntimePhysicsRegistration(CharacterComponentRuntime runtime, bool refreshOwner)
	{
		if (runtime == null || runtime.IsReleased || !_registrationByRuntime.ContainsKey(runtime) || !runtime.HasRuntimePhysicsWork)
		{
			RemovePhysicsRuntime(runtime);
		}
		else if (_physicsRuntimeIndices.TryAdd(runtime, _physicsRuntimes.Count))
		{
			_physicsRuntimes.Add(runtime);
		}
		if (refreshOwner)
		{
			RefreshOwnerProcessMembership();
		}
	}

	private void RemovePhysicsRuntime(CharacterComponentRuntime runtime)
	{
		if (runtime != null && _physicsRuntimeIndices.TryGetValue(runtime, out var value))
		{
			RemovePhysicsRuntimeAtSwap(value);
		}
	}

	private void RemovePhysicsRuntimeAtSwap(int index)
	{
		int num = _physicsRuntimes.Count - 1;
		CharacterComponentRuntime key = _physicsRuntimes[index];
		if (index != num)
		{
			CharacterComponentRuntime characterComponentRuntime = _physicsRuntimes[num];
			_physicsRuntimes[index] = characterComponentRuntime;
			_physicsRuntimeIndices[characterComponentRuntime] = index;
		}
		_physicsRuntimes.RemoveAt(num);
		_physicsRuntimeIndices.Remove(key);
	}

	private void RefreshRuntimeInputRegistration(CharacterComponentRuntime runtime, bool refreshOwner)
	{
		if (runtime != null && _registrationByRuntime.TryGetValue(runtime, out var value))
		{
			bool hasRuntimeInputWork = runtime.HasRuntimeInputWork;
			if (value.HasInputWork != hasRuntimeInputWork)
			{
				_inputRuntimeCount += (hasRuntimeInputWork ? 1 : (-1));
				value.HasInputWork = hasRuntimeInputWork;
			}
			bool hasRuntimeUnhandledInputWork = runtime.HasRuntimeUnhandledInputWork;
			if (value.HasUnhandledInputWork != hasRuntimeUnhandledInputWork)
			{
				_unhandledInputRuntimeCount += (hasRuntimeUnhandledInputWork ? 1 : (-1));
				value.HasUnhandledInputWork = hasRuntimeUnhandledInputWork;
			}
		}
		if (refreshOwner)
		{
			RefreshOwnerProcessMembership();
		}
	}

	private void ClearRuntimeInputWorkRegistration(CharacterComponentRuntime runtime)
	{
		if (runtime != null && _registrationByRuntime.TryGetValue(runtime, out var value))
		{
			if (value.HasInputWork)
			{
				value.HasInputWork = false;
				_inputRuntimeCount--;
			}
			if (value.HasUnhandledInputWork)
			{
				value.HasUnhandledInputWork = false;
				_unhandledInputRuntimeCount--;
			}
		}
	}

	private void RefreshRuntimeStateMachineRegistration(CharacterComponentRuntime runtime, bool refreshOwner)
	{
		if (runtime == null || runtime.IsReleased || !_registrationByRuntime.ContainsKey(runtime) || !runtime.HasStateMachine)
		{
			RemoveStatefulRuntime(runtime);
		}
		else if (_statefulRuntimeIndices.TryAdd(runtime, _statefulRuntimes.Count))
		{
			_statefulRuntimes.Add(runtime);
		}
		RefreshRuntimeStateMachineWorkRegistration(runtime);
		if (refreshOwner)
		{
			RefreshOwnerProcessMembership();
		}
	}

	private void RemoveStatefulRuntime(CharacterComponentRuntime runtime)
	{
		ClearRuntimeStateMachineWorkRegistration(runtime);
		if (runtime != null && _statefulRuntimeIndices.TryGetValue(runtime, out var value))
		{
			RemoveStatefulRuntimeAtSwap(value);
		}
	}

	private void RemoveStatefulRuntimeAtSwap(int index)
	{
		int num = _statefulRuntimes.Count - 1;
		CharacterComponentRuntime characterComponentRuntime = _statefulRuntimes[index];
		ClearRuntimeStateMachineWorkRegistration(characterComponentRuntime);
		if (index != num)
		{
			CharacterComponentRuntime characterComponentRuntime2 = _statefulRuntimes[num];
			_statefulRuntimes[index] = characterComponentRuntime2;
			_statefulRuntimeIndices[characterComponentRuntime2] = index;
		}
		_statefulRuntimes.RemoveAt(num);
		_statefulRuntimeIndices.Remove(characterComponentRuntime);
	}

	internal void RefreshRuntimeStateMachineWorkRegistration(CharacterComponentRuntime runtime)
	{
		if (runtime != null && _registrationByRuntime.TryGetValue(runtime, out var value))
		{
			bool hasStateMachineProcessWork = runtime.HasStateMachineProcessWork;
			if (value.HasProcessWork != hasStateMachineProcessWork)
			{
				_processStateMachineCount += (hasStateMachineProcessWork ? 1 : (-1));
				value.HasProcessWork = hasStateMachineProcessWork;
			}
			bool hasStateMachinePhysicsWork = runtime.HasStateMachinePhysicsWork;
			if (value.HasPhysicsWork != hasStateMachinePhysicsWork)
			{
				UpdatePhysicsStatefulRuntimeMembership(runtime, value, hasStateMachinePhysicsWork);
				_physicsStateMachineCount += (hasStateMachinePhysicsWork ? 1 : (-1));
				value.HasPhysicsWork = hasStateMachinePhysicsWork;
			}
		}
	}

	private void ClearRuntimeStateMachineWorkRegistration(CharacterComponentRuntime runtime)
	{
		if (runtime != null && _registrationByRuntime.TryGetValue(runtime, out var value))
		{
			if (value.HasProcessWork)
			{
				value.HasProcessWork = false;
				_processStateMachineCount--;
			}
			if (value.HasPhysicsWork)
			{
				UpdatePhysicsStatefulRuntimeMembership(runtime, value, hasPhysicsWork: false);
				value.HasPhysicsWork = false;
				_physicsStateMachineCount--;
			}
		}
	}

	private void UpdatePhysicsStatefulRuntimeMembership(CharacterComponentRuntime runtime, ComponentRegistration registration, bool hasPhysicsWork)
	{
		if (runtime == null || registration == null)
		{
			return;
		}
		if (hasPhysicsWork)
		{
			if (registration.PhysicsStatefulIndex < 0)
			{
				if (_physicsStatefulRuntimes == null)
				{
					_physicsStatefulRuntimes = new List<CharacterComponentRuntime>();
				}
				registration.PhysicsStatefulIndex = _physicsStatefulRuntimes.Count;
				_physicsStatefulRuntimes.Add(runtime);
			}
		}
		else if (registration.PhysicsStatefulIndex >= 0)
		{
			RemovePhysicsStatefulRuntimeAtSwap(registration.PhysicsStatefulIndex);
		}
	}

	private void RemovePhysicsStatefulRuntimeAtSwap(int index)
	{
		int num = _physicsStatefulRuntimes.Count - 1;
		CharacterComponentRuntime key = _physicsStatefulRuntimes[index];
		ComponentRegistration componentRegistration = _registrationByRuntime[key];
		if (index != num)
		{
			CharacterComponentRuntime characterComponentRuntime = _physicsStatefulRuntimes[num];
			_physicsStatefulRuntimes[index] = characterComponentRuntime;
			_registrationByRuntime[characterComponentRuntime].PhysicsStatefulIndex = index;
		}
		_physicsStatefulRuntimes.RemoveAt(num);
		componentRegistration.PhysicsStatefulIndex = -1;
	}

	private void RemoveStatefulComponent(ComponentBase component)
	{
		ClearStateMachineWorkRegistration(component);
		if (component != null && _statefulIndices.TryGetValue(component, out var value))
		{
			RemoveStatefulAtSwap(value);
		}
	}

	private void RemoveStatefulAtSwap(int index)
	{
		int num = _statefulComponents.Count - 1;
		ComponentBase componentBase = _statefulComponents[index];
		ClearStateMachineWorkRegistration(componentBase);
		if (index != num)
		{
			ComponentBase componentBase2 = _statefulComponents[num];
			_statefulComponents[index] = componentBase2;
			_statefulIndices[componentBase2] = index;
		}
		_statefulComponents.RemoveAt(num);
		_statefulIndices.Remove(componentBase);
	}

	private void RefreshStateMachineWorkRegistration(ComponentBase component)
	{
		if (component != null && _registrationByComponent.TryGetValue(component, out var value))
		{
			bool hasStateMachineProcessWork = component.HasStateMachineProcessWork;
			if (value.HasProcessWork != hasStateMachineProcessWork)
			{
				_processStateMachineCount += (hasStateMachineProcessWork ? 1 : (-1));
				value.HasProcessWork = hasStateMachineProcessWork;
			}
			bool hasStateMachinePhysicsWork = component.HasStateMachinePhysicsWork;
			if (value.HasPhysicsWork != hasStateMachinePhysicsWork)
			{
				UpdatePhysicsStatefulComponentMembership(component, value, hasStateMachinePhysicsWork);
				_physicsStateMachineCount += (hasStateMachinePhysicsWork ? 1 : (-1));
				value.HasPhysicsWork = hasStateMachinePhysicsWork;
			}
		}
	}

	private void ClearStateMachineWorkRegistration(ComponentBase component)
	{
		if (component != null && _registrationByComponent.TryGetValue(component, out var value))
		{
			if (value.HasProcessWork)
			{
				value.HasProcessWork = false;
				_processStateMachineCount--;
			}
			if (value.HasPhysicsWork)
			{
				UpdatePhysicsStatefulComponentMembership(component, value, hasPhysicsWork: false);
				value.HasPhysicsWork = false;
				_physicsStateMachineCount--;
			}
		}
	}

	private void UpdatePhysicsStatefulComponentMembership(ComponentBase component, ComponentRegistration registration, bool hasPhysicsWork)
	{
		if (component == null || registration == null)
		{
			return;
		}
		if (hasPhysicsWork)
		{
			if (registration.PhysicsStatefulIndex < 0)
			{
				if (_physicsStatefulComponents == null)
				{
					_physicsStatefulComponents = new List<ComponentBase>();
				}
				registration.PhysicsStatefulIndex = _physicsStatefulComponents.Count;
				_physicsStatefulComponents.Add(component);
			}
		}
		else if (registration.PhysicsStatefulIndex >= 0)
		{
			RemovePhysicsStatefulComponentAtSwap(registration.PhysicsStatefulIndex);
		}
	}

	private void RemovePhysicsStatefulComponentAtSwap(int index)
	{
		int num = _physicsStatefulComponents.Count - 1;
		ComponentBase key = _physicsStatefulComponents[index];
		ComponentRegistration componentRegistration = _registrationByComponent[key];
		if (index != num)
		{
			ComponentBase componentBase = _physicsStatefulComponents[num];
			_physicsStatefulComponents[index] = componentBase;
			_registrationByComponent[componentBase].PhysicsStatefulIndex = index;
		}
		_physicsStatefulComponents.RemoveAt(num);
		componentRegistration.PhysicsStatefulIndex = -1;
	}

	private void RefreshOwnerProcessMembership()
	{
		if (!_isExitingTree)
		{
			RefreshInputProcessMode();
			if (parent is TowerDefenseCharacter towerDefenseCharacter && GodotObject.IsInstanceValid(towerDefenseCharacter))
			{
				towerDefenseCharacter.RefreshComponentStateMachineDispatch();
			}
		}
	}

	private void RefreshInputProcessMode()
	{
		TowerDefenseCharacter owner = Owner;
		bool flag = IsOwnerInsideTree && GodotObject.IsInstanceValid(owner) && owner.HasValidRuntimeConfiguration && owner.inGame && !owner.editorPreviewMode && !owner.isDestroy;
		if (GodotObject.IsInstanceValid(owner))
		{
			owner.SetProcessInput(flag && HasRuntimeInputWork);
			owner.SetProcessUnhandledInput(flag && HasRuntimeUnhandledInputWork);
		}
	}

	private static string GetComponentTypeName(ComponentBase component)
	{
		string text = component._GetName();
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return component.GetType().Name;
	}

	public ComponentBase GetComponentFromName(string _name)
	{
		ComponentBase componentBase = FindComponentByNodeName(_name);
		if (GodotObject.IsInstanceValid(componentBase))
		{
			return componentBase;
		}
		string text = ResolveLegacyComponentName(_name);
		if (!(text == _name))
		{
			return FindComponentByNodeName(text);
		}
		return null;
	}

	public ComponentBase GetComponentFromType(string _type, int id = 0)
	{
		if (id < 0)
		{
			return null;
		}
		_type = ResolveLegacyComponentName(_type);
		if (componentDictionary.TryGetValue(_type, out var value))
		{
			for (int num = value.Count - 1; num >= 0; num--)
			{
				if (!GodotObject.IsInstanceValid(value[num]))
				{
					value.RemoveAt(num);
				}
			}
			if (id < value.Count)
			{
				return value[id];
			}
		}
		return null;
	}

	private ComponentBase FindComponentByNodeName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		for (int i = 0; i < componentList.Count; i++)
		{
			ComponentBase componentBase = componentList[i];
			if (GodotObject.IsInstanceValid(componentBase) && componentBase.Name == (StringName)name)
			{
				return componentBase;
			}
		}
		return null;
	}

	internal static string ResolveLegacyComponentName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return name;
		}
		if (LegacyComponentAliases.TryGetValue(name, out var value))
		{
			return value;
		}
		foreach (KeyValuePair<string, string> legacyComponentAlias in LegacyComponentAliases)
		{
			if (!name.StartsWith(legacyComponentAlias.Key, StringComparison.Ordinal) || name.Length == legacyComponentAlias.Key.Length)
			{
				continue;
			}
			bool flag = true;
			for (int i = legacyComponentAlias.Key.Length; i < name.Length; i++)
			{
				if (!char.IsAsciiDigit(name[i]))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				string value2 = legacyComponentAlias.Value;
				int length = legacyComponentAlias.Key.Length;
				return value2 + name.Substring(length, name.Length - length);
			}
		}
		return name;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		return new List<MethodInfo>(61)
		{
			new MethodInfo(MethodName.CancelAttachmentTransaction, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.CaptureAttachmentNetworkState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "wireKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IsInsideTree, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.GetParent, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.AddChild, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "child", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
			}, null),
			new MethodInfo(MethodName.RemoveChild, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "child", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
			}, null),
			new MethodInfo(MethodName.AttachOwner, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "owner", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.DetachOwner, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "reason", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.Release, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName._Notification, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "what", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.InitializeResourceComponents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ActivateResourceComponents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.NotifyOwnerGameplayActivated, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.NotifyOwnerBeforeDestroy, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.ScheduleResourceStateRegistration, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RegisterResourceStateRuntimes, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RebindResourceComponents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "owner", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.DetachResourceComponents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "reason", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ReleaseResourceComponents, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "reason", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RegisterExistingChildren, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.OnChildEnteredTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "child", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
			}, null),
			new MethodInfo(MethodName.OnChildExitingTree, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "child", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node"), exported: false)
			}, null),
			new MethodInfo(MethodName.RegisterComponent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.UnregisterComponent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.AttachRegisteredComponent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ApplyOrQueueNetworkState, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "wireKey", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Dictionary, "data", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RefreshStateMachineRegistration, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.NotifyComponentAliveChanged, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.DispatchRuntimeInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
			}, null),
			new MethodInfo(MethodName._Input, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
			}, null),
			new MethodInfo(MethodName.DispatchRuntimeUnhandledInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
			}, null),
			new MethodInfo(MethodName._UnhandledInput, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "inputEvent", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("InputEvent"), exported: false)
			}, null),
			new MethodInfo(MethodName.TickStateMachineProcess, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.TickStateMachinePhysics, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "ownerInsideComponentBattlefield", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetComponentStatePhysicsMetricName, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.TickRuntimePhysics, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "physicsFrame", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "ownerInsideComponentBattlefield", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.TickComponentTimers, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Float, "delta", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "ownerInsideComponentBattlefield", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.StartComponentTimer, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.StringName, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Float, "duration", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Bool, "useOwnerTimeScale", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.StopComponentTimer, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.StringName, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.PauseComponentTimer, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.StringName, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ResumeComponentTimer, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.StringName, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.IsComponentTimerRunning, new PropertyInfo(Variant.Type.Bool, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.StringName, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetComponentTimerRemaining, new PropertyInfo(Variant.Type.Float, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.StringName, "timerName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RegisterWireSlot, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false),
				new PropertyInfo(Variant.Type.String, "typeName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.BuildWireKey, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "typeName", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "typeIndex", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.DeactivateWireSlot, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.RemovePhysicsRuntimeAtSwap, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RemoveStatefulRuntimeAtSwap, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RemovePhysicsStatefulRuntimeAtSwap, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RemoveStatefulComponent, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.RemoveStatefulAtSwap, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RefreshStateMachineWorkRegistration, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.ClearStateMachineWorkRegistration, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.RemovePhysicsStatefulComponentAtSwap, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Int, "index", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.RefreshOwnerProcessMembership, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.RefreshInputProcessMode, new PropertyInfo(Variant.Type.Nil, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal, null, null),
			new MethodInfo(MethodName.GetComponentTypeName, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.Object, "component", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false)
			}, null),
			new MethodInfo(MethodName.GetComponentFromName, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "_name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.GetComponentFromType, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "_type", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false),
				new PropertyInfo(Variant.Type.Int, "id", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.FindComponentByNodeName, new PropertyInfo(Variant.Type.Object, "", PropertyHint.None, "", PropertyUsageFlags.Default, new StringName("Node2D"), exported: false), MethodFlags.Normal, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null),
			new MethodInfo(MethodName.ResolveLegacyComponentName, new PropertyInfo(Variant.Type.String, "", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false), MethodFlags.Normal | MethodFlags.Static, new List<PropertyInfo>
			{
				new PropertyInfo(Variant.Type.String, "name", PropertyHint.None, "", PropertyUsageFlags.Default, exported: false)
			}, null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.CancelAttachmentTransaction && args.Count == 0)
		{
			CancelAttachmentTransaction();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.CaptureAttachmentNetworkState && args.Count == 2)
		{
			CaptureAttachmentNetworkState(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Dictionary>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.IsInsideTree && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<bool>(IsInsideTree());
			return true;
		}
		if (method == MethodName.GetParent && args.Count == 0)
		{
			ret = VariantUtils.CreateFrom<Node>(GetParent());
			return true;
		}
		if (method == MethodName.AddChild && args.Count == 1)
		{
			AddChild(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveChild && args.Count == 1)
		{
			RemoveChild(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AttachOwner && args.Count == 1)
		{
			AttachOwner(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DetachOwner && args.Count == 1)
		{
			DetachOwner(VariantUtils.ConvertTo<ComponentDetachReason>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.Release && args.Count == 0)
		{
			Release();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Notification && args.Count == 1)
		{
			_Notification(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.InitializeResourceComponents && args.Count == 0)
		{
			InitializeResourceComponents();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ActivateResourceComponents && args.Count == 0)
		{
			ActivateResourceComponents();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.NotifyOwnerGameplayActivated && args.Count == 0)
		{
			NotifyOwnerGameplayActivated();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.NotifyOwnerBeforeDestroy && args.Count == 0)
		{
			NotifyOwnerBeforeDestroy();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ScheduleResourceStateRegistration && args.Count == 0)
		{
			ScheduleResourceStateRegistration();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RegisterResourceStateRuntimes && args.Count == 0)
		{
			RegisterResourceStateRuntimes();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RebindResourceComponents && args.Count == 1)
		{
			RebindResourceComponents(VariantUtils.ConvertTo<TowerDefenseCharacter>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DetachResourceComponents && args.Count == 1)
		{
			DetachResourceComponents(VariantUtils.ConvertTo<ComponentDetachReason>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ReleaseResourceComponents && args.Count == 1)
		{
			ReleaseResourceComponents(VariantUtils.ConvertTo<ComponentDetachReason>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RegisterExistingChildren && args.Count == 0)
		{
			RegisterExistingChildren();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnChildEnteredTree && args.Count == 1)
		{
			OnChildEnteredTree(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.OnChildExitingTree && args.Count == 1)
		{
			OnChildExitingTree(VariantUtils.ConvertTo<Node>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RegisterComponent && args.Count == 1)
		{
			RegisterComponent(VariantUtils.ConvertTo<ComponentBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.UnregisterComponent && args.Count == 1)
		{
			UnregisterComponent(VariantUtils.ConvertTo<ComponentBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.AttachRegisteredComponent && args.Count == 1)
		{
			AttachRegisteredComponent(VariantUtils.ConvertTo<ComponentBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ApplyOrQueueNetworkState && args.Count == 2)
		{
			ApplyOrQueueNetworkState(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<Dictionary>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshStateMachineRegistration && args.Count == 1)
		{
			RefreshStateMachineRegistration(VariantUtils.ConvertTo<ComponentBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.NotifyComponentAliveChanged && args.Count == 1)
		{
			NotifyComponentAliveChanged(VariantUtils.ConvertTo<ComponentBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DispatchRuntimeInput && args.Count == 1)
		{
			DispatchRuntimeInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._Input && args.Count == 1)
		{
			_Input(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.DispatchRuntimeUnhandledInput && args.Count == 1)
		{
			DispatchRuntimeUnhandledInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName._UnhandledInput && args.Count == 1)
		{
			_UnhandledInput(VariantUtils.ConvertTo<InputEvent>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TickStateMachineProcess && args.Count == 1)
		{
			TickStateMachineProcess(VariantUtils.ConvertTo<double>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TickStateMachinePhysics && args.Count == 2)
		{
			TickStateMachinePhysics(VariantUtils.ConvertTo<double>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetComponentStatePhysicsMetricName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetComponentStatePhysicsMetricName(VariantUtils.ConvertTo<ComponentBase>(in args[0])));
			return true;
		}
		if (method == MethodName.TickRuntimePhysics && args.Count == 3)
		{
			TickRuntimePhysics(VariantUtils.ConvertTo<double>(in args[0]), VariantUtils.ConvertTo<ulong>(in args[1]), VariantUtils.ConvertTo<bool>(in args[2]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.TickComponentTimers && args.Count == 2)
		{
			TickComponentTimers(VariantUtils.ConvertTo<double>(in args[0]), VariantUtils.ConvertTo<bool>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.StartComponentTimer && args.Count == 4)
		{
			ret = VariantUtils.CreateFrom<bool>(StartComponentTimer(VariantUtils.ConvertTo<ComponentBase>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1]), VariantUtils.ConvertTo<double>(in args[2]), VariantUtils.ConvertTo<bool>(in args[3])));
			return true;
		}
		if (method == MethodName.StopComponentTimer && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(StopComponentTimer(VariantUtils.ConvertTo<ComponentBase>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1])));
			return true;
		}
		if (method == MethodName.PauseComponentTimer && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(PauseComponentTimer(VariantUtils.ConvertTo<ComponentBase>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1])));
			return true;
		}
		if (method == MethodName.ResumeComponentTimer && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(ResumeComponentTimer(VariantUtils.ConvertTo<ComponentBase>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1])));
			return true;
		}
		if (method == MethodName.IsComponentTimerRunning && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<bool>(IsComponentTimerRunning(VariantUtils.ConvertTo<ComponentBase>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1])));
			return true;
		}
		if (method == MethodName.GetComponentTimerRemaining && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<double>(GetComponentTimerRemaining(VariantUtils.ConvertTo<ComponentBase>(in args[0]), VariantUtils.ConvertTo<StringName>(in args[1])));
			return true;
		}
		if (method == MethodName.RegisterWireSlot && args.Count == 2)
		{
			RegisterWireSlot(VariantUtils.ConvertTo<ComponentBase>(in args[0]), VariantUtils.ConvertTo<string>(in args[1]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.BuildWireKey && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<string>(BuildWireKey(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
		if (method == MethodName.DeactivateWireSlot && args.Count == 1)
		{
			DeactivateWireSlot(VariantUtils.ConvertTo<ComponentBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemovePhysicsRuntimeAtSwap && args.Count == 1)
		{
			RemovePhysicsRuntimeAtSwap(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveStatefulRuntimeAtSwap && args.Count == 1)
		{
			RemoveStatefulRuntimeAtSwap(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemovePhysicsStatefulRuntimeAtSwap && args.Count == 1)
		{
			RemovePhysicsStatefulRuntimeAtSwap(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveStatefulComponent && args.Count == 1)
		{
			RemoveStatefulComponent(VariantUtils.ConvertTo<ComponentBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemoveStatefulAtSwap && args.Count == 1)
		{
			RemoveStatefulAtSwap(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshStateMachineWorkRegistration && args.Count == 1)
		{
			RefreshStateMachineWorkRegistration(VariantUtils.ConvertTo<ComponentBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.ClearStateMachineWorkRegistration && args.Count == 1)
		{
			ClearStateMachineWorkRegistration(VariantUtils.ConvertTo<ComponentBase>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RemovePhysicsStatefulComponentAtSwap && args.Count == 1)
		{
			RemovePhysicsStatefulComponentAtSwap(VariantUtils.ConvertTo<int>(in args[0]));
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshOwnerProcessMembership && args.Count == 0)
		{
			RefreshOwnerProcessMembership();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.RefreshInputProcessMode && args.Count == 0)
		{
			RefreshInputProcessMode();
			ret = default(godot_variant);
			return true;
		}
		if (method == MethodName.GetComponentTypeName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetComponentTypeName(VariantUtils.ConvertTo<ComponentBase>(in args[0])));
			return true;
		}
		if (method == MethodName.GetComponentFromName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<ComponentBase>(GetComponentFromName(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.GetComponentFromType && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<ComponentBase>(GetComponentFromType(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
		if (method == MethodName.FindComponentByNodeName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<ComponentBase>(FindComponentByNodeName(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		if (method == MethodName.ResolveLegacyComponentName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ResolveLegacyComponentName(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		return base.InvokeGodotClassMethod(in method, args, out ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		if (method == MethodName.BuildWireKey && args.Count == 2)
		{
			ret = VariantUtils.CreateFrom<string>(BuildWireKey(VariantUtils.ConvertTo<string>(in args[0]), VariantUtils.ConvertTo<int>(in args[1])));
			return true;
		}
		if (method == MethodName.GetComponentTypeName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(GetComponentTypeName(VariantUtils.ConvertTo<ComponentBase>(in args[0])));
			return true;
		}
		if (method == MethodName.ResolveLegacyComponentName && args.Count == 1)
		{
			ret = VariantUtils.CreateFrom<string>(ResolveLegacyComponentName(VariantUtils.ConvertTo<string>(in args[0])));
			return true;
		}
		ret = default(godot_variant);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if (method == MethodName.CancelAttachmentTransaction)
		{
			return true;
		}
		if (method == MethodName.CaptureAttachmentNetworkState)
		{
			return true;
		}
		if (method == MethodName.IsInsideTree)
		{
			return true;
		}
		if (method == MethodName.GetParent)
		{
			return true;
		}
		if (method == MethodName.AddChild)
		{
			return true;
		}
		if (method == MethodName.RemoveChild)
		{
			return true;
		}
		if (method == MethodName.AttachOwner)
		{
			return true;
		}
		if (method == MethodName.DetachOwner)
		{
			return true;
		}
		if (method == MethodName.Release)
		{
			return true;
		}
		if (method == MethodName._Notification)
		{
			return true;
		}
		if (method == MethodName.InitializeResourceComponents)
		{
			return true;
		}
		if (method == MethodName.ActivateResourceComponents)
		{
			return true;
		}
		if (method == MethodName.NotifyOwnerGameplayActivated)
		{
			return true;
		}
		if (method == MethodName.NotifyOwnerBeforeDestroy)
		{
			return true;
		}
		if (method == MethodName.ScheduleResourceStateRegistration)
		{
			return true;
		}
		if (method == MethodName.RegisterResourceStateRuntimes)
		{
			return true;
		}
		if (method == MethodName.RebindResourceComponents)
		{
			return true;
		}
		if (method == MethodName.DetachResourceComponents)
		{
			return true;
		}
		if (method == MethodName.ReleaseResourceComponents)
		{
			return true;
		}
		if (method == MethodName.RegisterExistingChildren)
		{
			return true;
		}
		if (method == MethodName.OnChildEnteredTree)
		{
			return true;
		}
		if (method == MethodName.OnChildExitingTree)
		{
			return true;
		}
		if (method == MethodName.RegisterComponent)
		{
			return true;
		}
		if (method == MethodName.UnregisterComponent)
		{
			return true;
		}
		if (method == MethodName.AttachRegisteredComponent)
		{
			return true;
		}
		if (method == MethodName.ApplyOrQueueNetworkState)
		{
			return true;
		}
		if (method == MethodName.RefreshStateMachineRegistration)
		{
			return true;
		}
		if (method == MethodName.NotifyComponentAliveChanged)
		{
			return true;
		}
		if (method == MethodName.DispatchRuntimeInput)
		{
			return true;
		}
		if (method == MethodName._Input)
		{
			return true;
		}
		if (method == MethodName.DispatchRuntimeUnhandledInput)
		{
			return true;
		}
		if (method == MethodName._UnhandledInput)
		{
			return true;
		}
		if (method == MethodName.TickStateMachineProcess)
		{
			return true;
		}
		if (method == MethodName.TickStateMachinePhysics)
		{
			return true;
		}
		if (method == MethodName.GetComponentStatePhysicsMetricName)
		{
			return true;
		}
		if (method == MethodName.TickRuntimePhysics)
		{
			return true;
		}
		if (method == MethodName.TickComponentTimers)
		{
			return true;
		}
		if (method == MethodName.StartComponentTimer)
		{
			return true;
		}
		if (method == MethodName.StopComponentTimer)
		{
			return true;
		}
		if (method == MethodName.PauseComponentTimer)
		{
			return true;
		}
		if (method == MethodName.ResumeComponentTimer)
		{
			return true;
		}
		if (method == MethodName.IsComponentTimerRunning)
		{
			return true;
		}
		if (method == MethodName.GetComponentTimerRemaining)
		{
			return true;
		}
		if (method == MethodName.RegisterWireSlot)
		{
			return true;
		}
		if (method == MethodName.BuildWireKey)
		{
			return true;
		}
		if (method == MethodName.DeactivateWireSlot)
		{
			return true;
		}
		if (method == MethodName.RemovePhysicsRuntimeAtSwap)
		{
			return true;
		}
		if (method == MethodName.RemoveStatefulRuntimeAtSwap)
		{
			return true;
		}
		if (method == MethodName.RemovePhysicsStatefulRuntimeAtSwap)
		{
			return true;
		}
		if (method == MethodName.RemoveStatefulComponent)
		{
			return true;
		}
		if (method == MethodName.RemoveStatefulAtSwap)
		{
			return true;
		}
		if (method == MethodName.RefreshStateMachineWorkRegistration)
		{
			return true;
		}
		if (method == MethodName.ClearStateMachineWorkRegistration)
		{
			return true;
		}
		if (method == MethodName.RemovePhysicsStatefulComponentAtSwap)
		{
			return true;
		}
		if (method == MethodName.RefreshOwnerProcessMembership)
		{
			return true;
		}
		if (method == MethodName.RefreshInputProcessMode)
		{
			return true;
		}
		if (method == MethodName.GetComponentTypeName)
		{
			return true;
		}
		if (method == MethodName.GetComponentFromName)
		{
			return true;
		}
		if (method == MethodName.GetComponentFromType)
		{
			return true;
		}
		if (method == MethodName.FindComponentByNodeName)
		{
			return true;
		}
		if (method == MethodName.ResolveLegacyComponentName)
		{
			return true;
		}
		return base.HasGodotClassMethod(in method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if (name == PropertyName.ComponentSet)
		{
			ComponentSet = VariantUtils.ConvertTo<CharacterComponentSet>(in value);
			return true;
		}
		if (name == PropertyName.Name)
		{
			Name = VariantUtils.ConvertTo<StringName>(in value);
			return true;
		}
		if (name == PropertyName.ProcessMode)
		{
			ProcessMode = VariantUtils.ConvertTo<Node.ProcessModeEnum>(in value);
			return true;
		}
		if (name == PropertyName._attachmentOwner)
		{
			_attachmentOwner = VariantUtils.ConvertTo<TowerDefenseCharacter>(in value);
			return true;
		}
		if (name == PropertyName._resourceComponentSet)
		{
			_resourceComponentSet = VariantUtils.ConvertTo<CharacterComponentSet>(in value);
			return true;
		}
		if (name == PropertyName._processStateMachineCount)
		{
			_processStateMachineCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._physicsStateMachineCount)
		{
			_physicsStateMachineCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._inputRuntimeCount)
		{
			_inputRuntimeCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._unhandledInputRuntimeCount)
		{
			_unhandledInputRuntimeCount = VariantUtils.ConvertTo<int>(in value);
			return true;
		}
		if (name == PropertyName._isExitingTree)
		{
			_isExitingTree = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._resourceStateRegistrationScheduled)
		{
			_resourceStateRegistrationScheduled = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName._ownerAttached)
		{
			_ownerAttached = VariantUtils.ConvertTo<bool>(in value);
			return true;
		}
		if (name == PropertyName.parent)
		{
			parent = VariantUtils.ConvertTo<Node>(in value);
			return true;
		}
		return base.SetGodotClassPropertyValue(in name, in value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		if (name == PropertyName.ComponentSet)
		{
			value = VariantUtils.CreateFrom<CharacterComponentSet>(ComponentSet);
			return true;
		}
		if (name == PropertyName.Owner)
		{
			value = VariantUtils.CreateFrom<TowerDefenseCharacter>(Owner);
			return true;
		}
		bool from;
		if (name == PropertyName.IsOwnerInsideTree)
		{
			from = IsOwnerInsideTree;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.Name)
		{
			value = VariantUtils.CreateFrom<StringName>(Name);
			return true;
		}
		if (name == PropertyName.ProcessMode)
		{
			value = VariantUtils.CreateFrom<Node.ProcessModeEnum>(ProcessMode);
			return true;
		}
		if (name == PropertyName.HasStateMachineProcessWork)
		{
			from = HasStateMachineProcessWork;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HasStateMachinePhysicsWork)
		{
			from = HasStateMachinePhysicsWork;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HasRuntimePhysicsWork)
		{
			from = HasRuntimePhysicsWork;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HasComponentTimerWork)
		{
			from = HasComponentTimerWork;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HasRuntimeInputWork)
		{
			from = HasRuntimeInputWork;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.HasRuntimeUnhandledInputWork)
		{
			from = HasRuntimeUnhandledInputWork;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName.CanDispatchOwnerGameplay)
		{
			from = CanDispatchOwnerGameplay;
			value = VariantUtils.CreateFrom(in from);
			return true;
		}
		if (name == PropertyName._attachmentOwner)
		{
			value = VariantUtils.CreateFrom(in _attachmentOwner);
			return true;
		}
		if (name == PropertyName._resourceComponentSet)
		{
			value = VariantUtils.CreateFrom(in _resourceComponentSet);
			return true;
		}
		if (name == PropertyName._processStateMachineCount)
		{
			value = VariantUtils.CreateFrom(in _processStateMachineCount);
			return true;
		}
		if (name == PropertyName._physicsStateMachineCount)
		{
			value = VariantUtils.CreateFrom(in _physicsStateMachineCount);
			return true;
		}
		if (name == PropertyName._inputRuntimeCount)
		{
			value = VariantUtils.CreateFrom(in _inputRuntimeCount);
			return true;
		}
		if (name == PropertyName._unhandledInputRuntimeCount)
		{
			value = VariantUtils.CreateFrom(in _unhandledInputRuntimeCount);
			return true;
		}
		if (name == PropertyName._isExitingTree)
		{
			value = VariantUtils.CreateFrom(in _isExitingTree);
			return true;
		}
		if (name == PropertyName._resourceStateRegistrationScheduled)
		{
			value = VariantUtils.CreateFrom(in _resourceStateRegistrationScheduled);
			return true;
		}
		if (name == PropertyName._ownerAttached)
		{
			value = VariantUtils.CreateFrom(in _ownerAttached);
			return true;
		}
		if (name == PropertyName.parent)
		{
			value = VariantUtils.CreateFrom(in parent);
			return true;
		}
		return base.GetGodotClassPropertyValue(in name, out value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		return new List<PropertyInfo>
		{
			new PropertyInfo(Variant.Type.Object, PropertyName._attachmentOwner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.ComponentSet, PropertyHint.ResourceType, "CharacterComponentSet", PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable, exported: true),
			new PropertyInfo(Variant.Type.Object, PropertyName._resourceComponentSet, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._processStateMachineCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._physicsStateMachineCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._inputRuntimeCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName._unhandledInputRuntimeCount, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._isExitingTree, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._resourceStateRegistrationScheduled, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName._ownerAttached, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.parent, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Object, PropertyName.Owner, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.IsOwnerInsideTree, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.StringName, PropertyName.Name, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Int, PropertyName.ProcessMode, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.HasStateMachineProcessWork, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.HasStateMachinePhysicsWork, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.HasRuntimePhysicsWork, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.HasComponentTimerWork, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.HasRuntimeInputWork, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.HasRuntimeUnhandledInputWork, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false),
			new PropertyInfo(Variant.Type.Bool, PropertyName.CanDispatchOwnerGameplay, PropertyHint.None, "", PropertyUsageFlags.ScriptVariable, exported: false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		base.SaveGodotObjectData(info);
		info.AddProperty(PropertyName.ComponentSet, Variant.From<CharacterComponentSet>(ComponentSet));
		info.AddProperty(PropertyName.Name, Variant.From<StringName>(Name));
		info.AddProperty(PropertyName.ProcessMode, Variant.From<Node.ProcessModeEnum>(ProcessMode));
		info.AddProperty(PropertyName._attachmentOwner, Variant.From(in _attachmentOwner));
		info.AddProperty(PropertyName._resourceComponentSet, Variant.From(in _resourceComponentSet));
		info.AddProperty(PropertyName._processStateMachineCount, Variant.From(in _processStateMachineCount));
		info.AddProperty(PropertyName._physicsStateMachineCount, Variant.From(in _physicsStateMachineCount));
		info.AddProperty(PropertyName._inputRuntimeCount, Variant.From(in _inputRuntimeCount));
		info.AddProperty(PropertyName._unhandledInputRuntimeCount, Variant.From(in _unhandledInputRuntimeCount));
		info.AddProperty(PropertyName._isExitingTree, Variant.From(in _isExitingTree));
		info.AddProperty(PropertyName._resourceStateRegistrationScheduled, Variant.From(in _resourceStateRegistrationScheduled));
		info.AddProperty(PropertyName._ownerAttached, Variant.From(in _ownerAttached));
		info.AddProperty(PropertyName.parent, Variant.From(in parent));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		base.RestoreGodotObjectData(info);
		if (info.TryGetProperty(PropertyName.ComponentSet, out var value))
		{
			ComponentSet = value.As<CharacterComponentSet>();
		}
		if (info.TryGetProperty(PropertyName.Name, out var value2))
		{
			Name = value2.As<StringName>();
		}
		if (info.TryGetProperty(PropertyName.ProcessMode, out var value3))
		{
			ProcessMode = value3.As<Node.ProcessModeEnum>();
		}
		if (info.TryGetProperty(PropertyName._attachmentOwner, out var value4))
		{
			_attachmentOwner = value4.As<TowerDefenseCharacter>();
		}
		if (info.TryGetProperty(PropertyName._resourceComponentSet, out var value5))
		{
			_resourceComponentSet = value5.As<CharacterComponentSet>();
		}
		if (info.TryGetProperty(PropertyName._processStateMachineCount, out var value6))
		{
			_processStateMachineCount = value6.As<int>();
		}
		if (info.TryGetProperty(PropertyName._physicsStateMachineCount, out var value7))
		{
			_physicsStateMachineCount = value7.As<int>();
		}
		if (info.TryGetProperty(PropertyName._inputRuntimeCount, out var value8))
		{
			_inputRuntimeCount = value8.As<int>();
		}
		if (info.TryGetProperty(PropertyName._unhandledInputRuntimeCount, out var value9))
		{
			_unhandledInputRuntimeCount = value9.As<int>();
		}
		if (info.TryGetProperty(PropertyName._isExitingTree, out var value10))
		{
			_isExitingTree = value10.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._resourceStateRegistrationScheduled, out var value11))
		{
			_resourceStateRegistrationScheduled = value11.As<bool>();
		}
		if (info.TryGetProperty(PropertyName._ownerAttached, out var value12))
		{
			_ownerAttached = value12.As<bool>();
		}
		if (info.TryGetProperty(PropertyName.parent, out var value13))
		{
			parent = value13.As<Node>();
		}
	}
}
