using ExpensesAPI.Data;
using ExpensesAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ExpensesAPI.Controllers
{
    [EnableCors("*", "*","*")]
    public class EntriesController : ApiController
    {
        [AcceptVerbs("GET")]
        [HttpGet]
        public IHttpActionResult GeEntries()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var entries = context.Entries.ToList();
                    return Ok(entries);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }

        [HttpPost]
        public IHttpActionResult PostEntry([FromBody]Entry entry)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                using (var context = new AppDbContext())
                {
                    context.Entries.Add(entry);
                    context.SaveChanges();
                    return Ok("Entry was created.");
                }
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }
        [AcceptVerbs("PUT")]
        [HttpPut]
        public IHttpActionResult UpdateEntry(int id, [FromBody]Entry entry)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != entry.ID)
                return BadRequest(ModelState);

            try
            {
                using (var context = new AppDbContext())
                {
                    var oldEntry = context.Entries.FirstOrDefault(n => n.ID == id);
                    if (oldEntry == null)
                    {
                        return NotFound();
                    }

                    oldEntry.Description = entry.Description;
                    oldEntry.IsExpense = entry.IsExpense;
                    oldEntry.Value = entry.Value;

                    context.SaveChanges();

                    return Ok("Entry was updated.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AcceptVerbs("GET")]
        [HttpGet]
        public string Testing(int id, string cow)
        {
            return "test success" +  cow;

        }

        [AcceptVerbs("GET")]
        [HttpGet]
        public IHttpActionResult GetEntryByID(int id)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    var Entry = context.Entries.FirstOrDefault(n => n.ID == id);
                    if (Entry == null)
                    {
                        return NotFound();
                    }

                    return Ok(Entry);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AcceptVerbs("DELETE")]
        [HttpDelete]
        public IHttpActionResult DeleteEntry(int id, [FromBody] Entry entry)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != entry.ID)
                return BadRequest(ModelState);

            try
            {
                using (var context = new AppDbContext())
                {
                    var Entry = context.Entries.FirstOrDefault(n => n.ID == id);
                    if (Entry == null)
                    {
                        return NotFound();
                    }

                    context.Entries.Remove(Entry);
                    context.SaveChanges();

                    return Ok("Entry was deleted.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
