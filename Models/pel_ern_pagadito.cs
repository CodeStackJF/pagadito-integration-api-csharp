using System;
using System.ComponentModel.DataAnnotations;

namespace pagadito_updater_service.Models
{
    public partial class pel_ern_pagadito
    {
        [Key]
        public string ern_codigo { get; set; }
        public int ern_codper { get; set; }
        public string ern_token { get; set; }
        public string ern_estado { get; set; }
        public string ern_status { get; set; }
        public DateTime ern_fecha_creacion { get; set; }
        public DateTime? ern_fecha_actualizacion { get; set; }
        public DateTime? ern_fecha_registro_pago { get; set; }
        public DateTime ern_fecha_verificacion {get; set;}
    }
}