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
    public string RawText { get; }

    public ASRMessage(ASRMessageStateEnum aSRMessageState, string text, string rawText)
    {
        ASRMessageState = aSRMessageState;
        Text = text;
        RawText = rawText;
    }
}
