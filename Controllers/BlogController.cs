
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BloggingEngine.Models;
using BloggingEngine.DataAccess;

namespace BloggingEngine.Controllers
{
    public class BlogController : Controller
    {
        private PostContext _postContext;

        public BlogController(PostContext postContext)
        {
            _postContext = postContext;
        }

        [UrlFilter]
        [Route("blog")]
        [HttpGet()]
        public IActionResult Index() {
            // get posts in descending order to see the newest posts first
            var posts = _postContext.Posts.OrderByDescending(p => p.Id).ToList();
            var blogpostListModel = new BlogPostList();
            blogpostListModel.BlogPosts = posts;
            return View(blogpostListModel);
        }

        [UrlFilter]
        [Route("blog/create")]
        [HttpGet()]
        public IActionResult Create() {
            var authors = _postContext.Authors.ToList();
            var blogpostWithAuthor = new PostWithAuthor() {
                AuthorList = authors
            };
            return View(blogpostWithAuthor);
        }

        [UrlFilter]
        [Route("blog/create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PostWithAuthor item) {

            // Validate form
            if (ModelState.IsValid) {
                // If author exists
                if(item.Author.Id != -1) {
                    // Create new blog post with existing author
                    var post = new BloggingEngine.DataAccess.Post() {
                        PostTitle = item.Post.PostTitle,
                        PostContent = item.Post.PostContent,
                        AuthorId = item.Author.Id,
                        PostDate = item.Post.PostDate
                    };    
                    // Add blog post to database
                    _postContext.Posts.Add(post);
                    _postContext.SaveChanges();

                // If author does not exist
                } else {
                    // Create new author
                    var author = new BloggingEngine.DataAccess.Author() {
                        FirstName = item.Author.FirstName,
                        LastName = item.Author.LastName
                    };
                    // Add author to database
                    _postContext.Authors.Add(author);
                    _postContext.SaveChanges();
                    // Get the id of the last added record
                    var lastAddedAuthorId = author.Id;
                    // Create new blog post
                    var post = new BloggingEngine.DataAccess.Post() {
                        PostTitle = item.Post.PostTitle,
                        PostContent = item.Post.PostContent,
                        AuthorId = lastAddedAuthorId,
                        PostDate = item.Post.PostDate
                    };
                    // Add blog post to database
                    _postContext.Posts.Add(post);
                    _postContext.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            // Recreate the post to show validation message (didn't find any other way, sorry)
            var authors = _postContext.Authors.ToList();
            var blogpostWithAuthor = new PostWithAuthor() {
                AuthorList = authors
            };
            item = blogpostWithAuthor;
            return View("Create", item);
        }

        [UrlFilter]
        [Route("blog/post/{id}")]
        [HttpGet()]
        public IActionResult Post([FromRoute]int id) {
            // Redirect to index if no post found
            // get the post by id from query param
            var post = _postContext.Posts.Find(id);
            // send user back to index if post does not exist
            if (post == null)
            {
                return RedirectToAction("Index");
            } 

            // get the author of the post
            var author = _postContext.Authors.Find(post.AuthorId);

            // get the comments of the post by post id
            var postComments = _postContext.Comments.Where(c => c.PostId == post.Id);
            var comments = postComments.ToList();  // je kan de .ToList rechtstreeks op het einde van bovenstaande lijn toevoegen

            // build author model
            var authorModel = new Author() {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            };

            // ik zou onderstaand model eerst maken, maar nog zonder PostAuthor en Comments. je kan deze dan nog later opvullen met de code van hierboven.

            // build blog post model
            var blogpostModel = new BlogPostModel() {
                Id = post.Id,
                PostTitle = post.PostTitle,
                PostContent = post.PostContent,
                AuthorId = post.AuthorId,
                PostAuthor = authorModel,
                PostDate = post.PostDate,
                Comments = comments
            };
            return View(blogpostModel);
        }

        [UrlFilter]
        [Route("blog/post/{id}")]
        [HttpPost]
        public IActionResult Delete([FromRoute] int id) {
            _postContext.Remove(_postContext.Posts.Find(id));
            _postContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [UrlFilter]
        [Route("blog/post/{id}/comment")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Comment([FromRoute] int id, BlogPostModel item) {
            // Validate Form
            ModelState.Remove("PostTitle");
            ModelState.Remove("PostContent");
            ModelState.Remove("PostDate");
            if (ModelState.IsValid) {
                var comment = new BloggingEngine.DataAccess.PostComment() {
                    CommentAuthor = item.Comment.CommentAuthor,
                    CommentContent = item.Comment.CommentContent,
                    PostId = id
                };
                _postContext.Comments.Add(comment);
                _postContext.SaveChanges();

                return RedirectToAction("Post");
            }

            // Recreate the post to show validation message (didn't find any other way, sorry)
            var post = _postContext.Posts.Find(id);
            var author = _postContext.Authors.Find(post.AuthorId); 
            var postComments = _postContext.Comments.Where(c => c.PostId == post.Id);
            var comments = postComments.ToList();
            var authorModel = new Author() {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            };
            var blogpostModel = new BlogPostModel() {
                Id = post.Id,
                PostTitle = post.PostTitle,
                PostContent = post.PostContent,
                AuthorId = post.AuthorId,
                PostAuthor = authorModel,
                PostDate = post.PostDate,
                Comments = comments
            };
            item = blogpostModel;
            return View("Post", item);
        }
        
        [UrlFilter]
        [Route("blog/edit/{id}")]
        [HttpGet()]
        public IActionResult Edit([FromRoute] int id) {
            // Redirect to index if no post found
            var post = _postContext.Posts.Find(id);
            if (post == null)
            {
                return RedirectToAction("Index");
            }
            var blogpostModel = new BlogPostModel() {
                Id = post.Id,
                PostTitle = post.PostTitle,
                PostContent = post.PostContent,
                AuthorId = post.AuthorId,
                PostDate = post.PostDate
            };
            return View(blogpostModel);
        }

        // The tutorial I followed to create this method:
        // https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api-mac?view=aspnetcore-2.1
        [UrlFilter]
        [Route("blog/edit/{id}")]
        [HttpPost()]
        [ValidateAntiForgeryToken]
        public IActionResult Update([FromRoute] int id, BlogPostModel item) {
            // Redirect to index if no post found
            var post = _postContext.Posts.Find(id);
            if (post == null)
            {
                return RedirectToAction("Index");
            }

            // Validate Form
            if (ModelState.IsValid) {
                post.PostTitle = item.PostTitle;
                post.PostContent = item.PostContent;
                post.PostDate = item.PostDate;

                _postContext.Posts.Update(post);
                _postContext.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("Edit", item);
        }
    }
}
