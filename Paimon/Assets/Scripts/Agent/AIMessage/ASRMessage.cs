public enum ASRMessageStateEnum
{
    Starting,
    Recognizing,
    Finish
}
public class ASRMessage : AIMessage
{
    public ASRMessageStateEnum ASRMessageState { get; }
    public string Text { get; }

    public ASRMessage(ASRMessageStateEnum aSRMessageState, string text)
    {
        ASRMessageState = aSRMessageState;
        Text = text;
    }
}
