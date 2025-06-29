using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.ApplicationServices.Implementations;
using BookLibrary.DataAccess.Contracts;
using DataAccess.Database;
using ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using DataAccess.Contracts;
using ApplicationServices.Contracts;
using System;
using System.Data;
using ApplicationServices.Implementations;
using BookLibrary.ViewModels;
using BookLibrary.ViewModels.AuthorManagement;
using BookLibrary.ViewModels;
using BookLibrary.ViewModels.GenreManagement;
using BookLibrary.ViewModels.WishlistManagement;
using DataAccess.Implementations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;

namespace BookLibrary.WPF
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        private IConfiguration _configuration;
        public static IHost AppHost { get; private set; }

        public App()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Database services
            services.AddTransient<IDbConnection>(sp =>
                new SqlConnection(_configuration.GetConnectionString("BookLibraryDB")));
            services.AddSingleton<DbConnectionFactory>(
                new DbConnectionFactory(_configuration.GetConnectionString("BookLibraryDB")));

            // Repository services
            services.AddTransient<IBookRepository, BookRepository>();
            services.AddTransient<IAuthorRepository, AuthorRepository>();
            services.AddTransient<IGenreRepository, GenreRepository>();
            services.AddTransient<IWishlistRepository, WishlistRepository>();

            // Application services
            services.AddTransient<IBookService, BookService>();
            services.AddTransient<IAuthorService, AuthorService>();
            services.AddTransient<IGenreService, GenreService>();
            services.AddTransient<IWishlistService, WishlistService>();
            services.AddTransient<IImageService, ImageService>(); // Add missing ImageService

            // ViewModels
            services.AddTransient<MainViewModel>();
            services.AddTransient<BookListViewModel>();
            services.AddTransient<BookDetailViewModel>(); 
            services.AddTransient<AuthorManagerViewModel>(); 
            services.AddTransient<GenreManagerViewModel>();  
            services.AddTransient<WishlistManagerViewModel>();

            // Windows
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetService<MainWindow>();
            var mainViewModel = _serviceProvider.GetService<MainViewModel>();
            
            // Set the MainViewModel as the DataContext for the MainWindow
            mainWindow.DataContext = mainViewModel;
            
            mainWindow.Show();
        }
    }
}
