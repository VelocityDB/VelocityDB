using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AutoClaimUsingSQL;

namespace AutoClaimWebService.Controllers
{
    public class MitchellClaimTypesController : ApiController
    {
        private AutoClaimEntityFrameworkContext db = new AutoClaimEntityFrameworkContext();

        // GET: api/MitchellClaimTypes
        public IQueryable<MitchellClaimType> GetClaims()
        {
            return db.Claims;
        }

        // GET: api/MitchellClaimTypes/5
        [ResponseType(typeof(MitchellClaimType))]
        public async Task<IHttpActionResult> GetMitchellClaimType(int id)
        {
            MitchellClaimType mitchellClaimType = await db.Claims.FindAsync(id);
            if (mitchellClaimType == null)
            {
                return NotFound();
            }

            return Ok(mitchellClaimType);
        }

        // PUT: api/MitchellClaimTypes/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutMitchellClaimType(int id, MitchellClaimType mitchellClaimType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != mitchellClaimType.MitchellClaimTypeId)
            {
                return BadRequest();
            }

            db.Entry(mitchellClaimType).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MitchellClaimTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/MitchellClaimTypes
        [ResponseType(typeof(MitchellClaimType))]
        public async Task<IHttpActionResult> PostMitchellClaimType(MitchellClaimType mitchellClaimType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Claims.Add(mitchellClaimType);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = mitchellClaimType.MitchellClaimTypeId }, mitchellClaimType);
        }

        // DELETE: api/MitchellClaimTypes/5
        [ResponseType(typeof(MitchellClaimType))]
        public async Task<IHttpActionResult> DeleteMitchellClaimType(int id)
        {
            MitchellClaimType mitchellClaimType = await db.Claims.FindAsync(id);
            if (mitchellClaimType == null)
            {
                return NotFound();
            }

            db.Claims.Remove(mitchellClaimType);
            await db.SaveChangesAsync();

            return Ok(mitchellClaimType);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MitchellClaimTypeExists(int id)
        {
            return db.Claims.Count(e => e.MitchellClaimTypeId == id) > 0;
        }
    }
}