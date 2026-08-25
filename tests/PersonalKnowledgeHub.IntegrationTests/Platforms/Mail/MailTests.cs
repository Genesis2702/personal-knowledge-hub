using System.Net;
using System.Net.Http.Json;
using PersonalKnowledgeHub.DTOs.Requests;
using PersonalKnowledgeHub.IntegrationTests.Infrastructure.Mail;

namespace PersonalKnowledgeHub.IntegrationTests.Platforms.Mail;

[Collection(nameof(MailCollection))]
public class MailTests
{
    private readonly MailFixture _fixture;
    
    public MailTests(MailFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Register_WhenCalled_SendMailToTestUser()
    {
        HttpRequestMessage request = new  HttpRequestMessage(HttpMethod.Post, "/auth/register");
        request.Content = JsonContent.Create(new RegisterRequestDto
        {
            UserName = "username",
            Email = "user@gmail.com",
            Password = "user password"
        });
        
        HttpResponseMessage response = await _fixture.Client!.SendAsync(request);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        MailpitMessage? message = null;

        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(15));

        while (!timeout.IsCancellationRequested)
        {
            using HttpResponseMessage mailResponse = await _fixture.MailpitClient.GetAsync("/api/v1/message/latest", timeout.Token);

            if (mailResponse.StatusCode == HttpStatusCode.OK)
            {
                message = await mailResponse.Content.ReadFromJsonAsync<MailpitMessage>(timeout.Token);

                if (message?.To.Any(address => address.Address == "user@gmail.com") == true)
                {
                    break;
                }
            }
            
            await Task.Delay(200, timeout.Token);
        }
        
        Assert.NotNull(message);
        Assert.Equal("sender@test.local", message.From.Address);
        Assert.Contains(message.To, address => address.Address == "user@gmail.com");
        Assert.Equal("Email Verification", message.Subject);
    }
}