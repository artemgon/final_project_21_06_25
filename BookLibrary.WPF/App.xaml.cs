using BookLibrary.ApplicationServices.Contracts;
using BookLibrary.ApplicationServices.Implementations;
using BookLibrary.DataAccess.Contracts;
using DataAccess.Database;
using BookLibrary.DataAccess.Implementations;
using ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using DataAccess.Contracts;
using ApplicationServices.Contracts;
using System;
using System.Data;
using System.Windows;
using ApplicationServices.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;

namespace BookLibrary.WPF
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        private IConfiguration _configuration;

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
            services.AddTransient<IDbConnection>(sp =>
                new SqlConnection(_configuration.GetConnectionString("BookLibraryDB")));
            services.AddSingleton<DbConnectionFactory>(
                new DbConnectionFactory(_configuration.GetConnectionString("BookLibraryDB")));

            services.AddTransient<IBookRepository, BookRepository>();
            services.AddTransient<IAuthorRepository, AuthorRepository>();
            services.AddTransient<IGenreRepository, GenreRepository>();
            services.AddTransient<IWishlistRepository, WishlistRepository>();

            services.AddTransient<IBookService, BookService>();
            services.AddTransient<IAuthorService, AuthorService>();
            services.AddTransient<IGenreService, GenreService>();
            services.AddTransient<IWishlistService, WishlistService>();

            services.AddTransient<MainViewModel>();
            services.AddTransient<BookListViewModel>();
            services.AddTransient<BookDetailViewModel>(); 
            services.AddTransient<AuthorManagerViewModel>(); 
            services.AddTransient<GenreManagerViewModel>();  
            services.AddTransient<WishlistManagerViewModel>();

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
