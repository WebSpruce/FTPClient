using FTPClient.Models.Models;

namespace FTPClient.ViewModels;

public interface IHomePageViewModelFactory
{
    HomePageViewModel Create(Connection? connection = null);
}