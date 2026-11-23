public readonly struct NetworkTransportMessage
{
	public NetMessageEnvelope Envelope { get; }

	public string SenderPeerId { get; }

	public NetDeliveryMode DeliveryMode { get; }

	public NetworkTransportMessage(NetMessageEnvelope envelope, string senderPeerId, NetDeliveryMode deliveryMode)
	{
		Envelope = envelope;
		SenderPeerId = senderPeerId;
		DeliveryMode = deliveryMode;
	}
}
