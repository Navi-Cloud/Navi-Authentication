namespace NaviAuth.Model.Data;

public class AccessToken
{
    public string Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public string UserId { get; set; }
}