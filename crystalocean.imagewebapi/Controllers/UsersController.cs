using System;
using System.Collections.Generic;
using System.Linq;
using CrystalOcean.Data.Models;
using Microsoft.AspNetCore.Mvc;
using CrystalOcean.Data.Models.Repository;

namespace CrystalOcean.ImageWebApi.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly UserRepository _userRepository;

        public UsersController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET api/users
        [HttpGet]
        public IEnumerable<User> Get()
        {
            return _userRepository.Users.ToList();
        }

        // GET api/users/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/users
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/users/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/users/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
