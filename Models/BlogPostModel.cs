using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BloggingEngine.DataAccess;

public class BlogPostModel
{  
    public int Id { get; set; }
    [Required(ErrorMessage = "Title field can not be empty")]

    public string PostTitle { get; set; }
    [Required(ErrorMessage = "Content field can not be empty")]

    public string PostContent { get; set; }
    public int AuthorId { get; set; }
    public Author PostAuthor { get; set; }
    [Required(ErrorMessage = "Date field can not be empty")]

    public string PostDate { get; set; }
    public List<PostComment> Comments { get; set; }
    public Comment Comment { get; set; }
}

// public class Author
//   {  
//     public int Id { get; set; }
//     public string FirstName { get; set; }
//     public string LastName { get; set; }
//   }

public class BlogPostList {
    public List<Post> BlogPosts { get; set; }
}

public class Comment {
    public int Id { get; set; }  
    [Required(ErrorMessage = "Author field can not be empty")]
    public string CommentAuthor { get; set; }
    [Required(ErrorMessage = "Comment field can not be empty")]
    public string CommentContent { get; set; }
    public int PostId { get; set; }

}

// To create a blog post and add a new author
public class PostWithAuthor {
    public List<Author> AuthorList { get; set; }
    public Author Author { get; set; }
    public BlogPostModel Post { get; set; }
}
