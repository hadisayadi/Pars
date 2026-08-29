"""
Pars Database Scaffolding Tool
Generates C# Entity Framework Core entities from HTML documentation
"""

import os
import re
import shutil
from pathlib import Path
from dataclasses import dataclass, field
from typing import Dict, List, Optional, Set
from bs4 import BeautifulSoup
from jinja2 import Environment, DictLoader

# ============================================================
# 1. DATA MODELS
# ============================================================

@dataclass
class Column:
    name: str
    sql_type: str
    csharp_type: str
    is_pk: bool = False
    is_nullable: bool = True
    max_length: Optional[int] = None

@dataclass
class Table:
    schema: str
    name: str
    columns: List[Column] = field(default_factory=list)
    primary_keys: List[str] = field(default_factory=list)
    class_name: str = ""
    
    def __post_init__(self):
        self.class_name = self._to_pascal_case(self.name)

@dataclass
class ForeignKey:
    source_table: str
    source_column: str
    target_table: str
    target_column: str

# ============================================================
# 2. SQL SERVER TO C# TYPE MAPPING
# ============================================================

TYPE_MAP = {
    'int': 'int',
    'bigint': 'long',
    'smallint': 'short',
    'tinyint': 'byte',
    'bit': 'bool',
    'real': 'float',
    'float': 'double',
    'date': 'DateOnly',
    'datetime': 'DateTime',
    'datetime2': 'DateTime',
    'time': 'TimeSpan',
    'image': 'byte[]',
    'ntext': 'string',
    'text': 'string',
}

def parse_sql_type(sql_type: str) -> tuple[str, Optional[int]]:
    """Parse SQL type and extract length"""
    sql_type = sql_type.strip().lower()
    
    # Handle numeric(18, 2), numeric(18, 0)
    m = re.match(r'numeric\s*\((\d+),\s*(\d+)\)', sql_type)
    if m:
        precision, scale = int(m.group(1)), int(m.group(2))
        if scale == 0:
            return 'long' if precision <= 18 else 'decimal', None
        return 'decimal', None
    
    # Handle nvarchar(N), nvarchar(max), varchar(N)
    m = re.match(r'(n?varchar|n?char)\s*\((\d+|max)\)', sql_type)
    if m:
        base = m.group(1).lower()
        length = m.group(2)
        if length == 'max':
            return 'string', None
        return 'string', int(length)
    
    # Handle binary(N), varbinary(max)
    m = re.match(r'(var)?binary\s*\((\d+|max)\)', sql_type)
    if m:
        length = m.group(2)
        return 'byte[]', None
    
    # Simple types
    for sql_t, csharp_t in TYPE_MAP.items():
        if sql_type.startswith(sql_t):
            return csharp_t, None
    
    return 'string', None

# ============================================================
# 3. NAMING UTILITIES
# ============================================================

RESERVED_WORDS = {
    'class', 'object', 'string', 'int', 'long', 'byte', 'short', 'float', 'double',
    'bool', 'decimal', 'char', 'void', 'namespace', 'using', 'public', 'private',
    'protected', 'internal', 'static', 'virtual', 'override', 'abstract', 'new',
    'if', 'else', 'for', 'foreach', 'while', 'do', 'switch', 'case', 'default',
    'return', 'break', 'continue', 'try', 'catch', 'finally', 'throw', 'base',
    'this', 'true', 'false', 'null', 'in', 'out', 'ref', 'params', 'is', 'as',
    'typeof', 'sizeof', 'lock', 'event', 'delegate', 'interface', 'struct',
    'enum', 'const', 'readonly', 'volatile', 'fixed', 'checked', 'unchecked',
    'operator', 'implicit', 'explicit', 'yield', 'async', 'await', 'dynamic',
    'where', 'from', 'select', 'group', 'by', 'into', 'join', 'on', 'equals',
    'let', 'orderby', 'ascending', 'descending', 'var', 'set', 'value', 'get'
}

def to_pascal_case(name: str) -> str:
    """Convert table name to PascalCase class name"""
    name = re.sub(r'^tbl', '', name, flags=re.IGNORECASE)
    name = re.sub(r'^_', '', name)
    
    parts = re.split(r'[_\-\s]+', name)
    result = ''.join(p.capitalize() for p in parts if p)
    
    if not result:
        result = 'Entity'
    
    # Ensure starts with letter
    if result[0].isdigit():
        result = 'T' + result
    
    return result

def to_camel_case(name: str) -> str:
    pascal = to_pascal_case(name)
    return pascal[0].lower() + pascal[1:] if pascal else pascal

def safe_property_name(name: str) -> str:
    """Convert column name to safe C# property name"""
    # Remove leading/trailing special chars
    name = re.sub(r'^[^a-zA-Z_]+|[^a-zA-Z0-9_]+$', '', name)
    name = re.sub(r'[^a-zA-Z0-9_]', '', name)
    
    if not name:
        name = 'Property'
    
    # Capitalize first letter for property
    prop_name = name[0].upper() + name[1:] if name else 'Property'
    
    # Check for reserved words
    if prop_name.lower() in RESERVED_WORDS:
        prop_name = '@' + prop_name
    
    return prop_name

# ============================================================
# 4. HTML PARSER
# ============================================================

class DatabaseParser:
    def __init__(self, html_content: str):
        self.soup = BeautifulSoup(html_content, 'lxml')
        self.tables: Dict[str, Table] = {}  # key: schema.name
        self.foreign_keys: List[ForeignKey] = []
    
    def parse(self):
        self._parse_table_list()
        self._parse_table_details()
        self._parse_foreign_keys()
    
    def _get_table_key(self, schema: str, name: str) -> str:
        return f"{schema}.{name}"
    
    def _parse_table_list(self):
        """Extract table list from first table"""
        h2 = self.soup.find('h2', string=re.compile('فهرست جداول'))
        if not h2:
            return
        
        table = h2.find_next('table')
        if not table:
            return
        
        rows = table.find_all('tr')[1:]  # skip header
        for row in rows:
            cols = row.find_all('td')
            if len(cols) >= 4:
                schema = cols[0].text.strip()
                name = cols[1].text.strip()
                pks = cols[3].text.strip()
                
                key = self._get_table_key(schema, name)
                self.tables[key] = Table(
                    schema=schema,
                    name=name,
                    primary_keys=[pk.strip() for pk in pks.split(',') if pk.strip()]
                )
    
    def _parse_table_details(self):
        """Parse detailed column info for each table"""
        for h3 in self.soup.find_all('h3'):
            full_name = h3.text.strip()
            if '.' not in full_name:
                continue
            
            schema, name = full_name.split('.', 1)
            key = self._get_table_key(schema, name)
            
            if key not in self.tables:
                continue
            
            table = self.tables[key]
            col_table = h3.find_next('table')
            if not col_table:
                continue
            
            rows = col_table.find_all('tr')[1:]
            for row in rows:
                cols = row.find_all('td')
                if len(cols) < 2:
                    continue
                
                col_name = cols[0].text.strip()
                sql_type = cols[1].text.strip()
                
                csharp_type, max_len = parse_sql_type(sql_type)
                is_pk = col_name in table.primary_keys
                
                # Non-PK columns should be nullable in C#
                is_nullable = not is_pk and csharp_type not in ('byte[]', 'string')
                
                col = Column(
                    name=col_name,
                    sql_type=sql_type,
                    csharp_type=csharp_type,
                    is_pk=is_pk,
                    is_nullable=is_nullable,
                    max_length=max_len
                )
                table.columns.append(col)
    
    def _parse_foreign_keys(self):
        """Parse FK relationships"""
        h2 = self.soup.find('h2', string=re.compile('روابط Foreign Key'))
        if not h2:
            return
        
        table = h2.find_next('table')
        if not table:
            return
        
        rows = table.find_all('tr')[1:]
        for row in rows:
            cols = row.find_all('td')
            if len(cols) >= 4:
                fk = ForeignKey(
                    source_table=cols[0].text.strip(),
                    source_column=cols[1].text.strip(),
                    target_table=cols[2].text.strip(),
                    target_column=cols[3].text.strip()
                )
                self.foreign_keys.append(fk)

# ============================================================
# 5. CODE GENERATOR
# ============================================================

class CodeGenerator:
    def __init__(self, parser: DatabaseParser, output_dir: str):
        self.parser = parser
        self.output_dir = Path(output_dir)
        self.entity_ns = "Pars.Domain.Entities"
        self.dto_ns = "Pars.Application.DTOs"
        
    def generate_all(self):
        print("🔨 Generating code...")
        
        # Create directory structure
        dirs = [
            'src/Pars.Domain/Entities',
            'src/Pars.Application/DTOs',
            'src/Pars.Infrastructure/Persistence',
            'src/Pars.API/Controllers',
        ]
        for d in dirs:
            (self.output_dir / d).mkdir(parents=True, exist_ok=True)
        
        # Generate entities
        for key, table in self.parser.tables.items():
            if not table.columns:
                continue
            self._generate_entity(table)
            self._generate_dto(table)
            self._generate_controller(table)
        
        # Generate DbContext with all relationships
        self._generate_dbcontext()
        
        print(f"\n✅ Generated {len(self.parser.tables)} entities")
        print(f"✅ Generated {len(self.parser.foreign_keys)} FK relationships")
        print(f"📁 Output: {self.output_dir}")
    
    def _generate_entity(self, table: Table):
        # Build FK info for this table
        fk_columns: Dict[str, ForeignKey] = {}
        incoming_fks: List[ForeignKey] = []
        
        for fk in self.parser.foreign_keys:
            src_key = fk.source_table
            tgt_key = fk.target_table
            
            full_name = f"{table.schema}.{table.name}"
            
            if src_key == full_name:
                fk_columns[fk.source_column] = fk
            
            if tgt_key == full_name and fk.source_table != full_name:
                incoming_fks.append(fk)
        
        template = """// Auto-generated by Pars Scaffolding
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace {{ namespace }};

[Table("{{ table.name }}", Schema = "{{ table.schema }}")]
public class {{ table.class_name }}
{
{% for col in table.columns %}
{% if col.is_pk %}    [Key]
{% endif %}    [Column("{{ col.name }}")]
{% if col.max_length %}    [StringLength({{ col.max_length }})]
{% endif %}    public {{ col.csharp_type }}{% if col.is_nullable %}?{% endif %} {{ col.prop_name }} { get; set; }{% if not col.is_nullable and col.csharp_type == 'string' %} = default!;{% elif not col.is_nullable %} = default;{% endif %}

{% endfor %}
    // Navigation Properties (Outgoing FKs)
{% for fk in fk_columns.values() %}
    [ForeignKey(nameof({{ fk.source_prop }}))]
    public virtual {{ fk.target_class }}? {{ fk.nav_name }} { get; set; }
{% endfor %}
{% if fk_columns %}
{% endif %}
    // Navigation Properties (Incoming FKs - Collections)
{% for fk in incoming_fks %}
    public virtual ICollection<{{ fk.source_class }}> {{ fk.collection_name }} { get; set; } = new List<{{ fk.source_class }}>();
{% endfor %}
}
"""
        # Prepare data
        for col in table.columns:
            col.prop_name = safe_property_name(col.name)
        
        for fk in fk_columns.values():
            fk.source_prop = safe_property_name(fk.source_column)
            fk.target_class = to_pascal_case(fk.target_table.split('.')[-1])
            fk.nav_name = fk.target_class
        
        for fk in incoming_fks:
            fk.source_class = to_pascal_case(fk.source_table.split('.')[-1])
            fk.collection_name = fk.source_class + 's'
        
        env = Environment(loader=DictLoader({'t': template}))
        content = env.get_template('t').render(
            namespace=self.entity_ns,
            table=table,
            fk_columns=fk_columns,
            incoming_fks=incoming_fks,
        )
        
        # Clean up excessive blank lines
        content = re.sub(r'\n{4,}', '\n\n\n', content)
        
        out_file = self.output_dir / 'src/Pars.Domain/Entities' / f'{table.class_name}.cs'
        out_file.write_text(content, encoding='utf-8')
    
    def _generate_dto(self, table: Table):
        template = """// Auto-generated by Pars Scaffolding
namespace {{ namespace }};

public record {{ table.class_name }}Dto(
{% for col in table.columns %}    {{ col.csharp_type }}{% if col.is_nullable or col.csharp_type == 'string' %}?{% endif %} {{ col.prop_name }}{% if not loop.last %},{% endif %}

{% endfor %});

public record Create{{ table.class_name }}Dto(
{% for col in table.columns if not col.is_pk %}    {{ col.csharp_type }}{% if col.is_nullable or col.csharp_type == 'string' %}?{% endif %} {{ col.prop_name }}{% if not loop.last %},{% endif %}

{% endfor %});
"""
        for col in table.columns:
            col.prop_name = safe_property_name(col.name)
        
        env = Environment(loader=DictLoader({'t': template}))
        content = env.get_template('t').render(
            namespace=self.dto_ns,
            table=table,
        )
        content = re.sub(r'\n{4,}', '\n\n\n', content)
        
        out_file = self.output_dir / 'src/Pars.Application/DTOs' / f'{table.class_name}Dto.cs'
        out_file.write_text(content, encoding='utf-8')
    
    def _generate_controller(self, table: Table):
        pk_cols = [c for c in table.columns if c.is_pk]
        if not pk_cols:
            return
        
        pk = pk_cols[0]
        pk_type = pk.csharp_type + ('?' if pk.is_nullable else '')
        pk_name = safe_property_name(pk.name)
        
        template = """// Auto-generated by Pars Scaffolding
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pars.Domain.Entities;
using Pars.Infrastructure.Persistence;

namespace Pars.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class {{ table.class_name }}sController : ControllerBase
{
    private readonly ParsDbContext _context;

    public {{ table.class_name }}sController(ParsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _context.Set<{{ table.class_name }}>().ToListAsync(ct);
        return Ok(items);
    }

    [HttpGet("{{'{'}}id{{'}'}}")]
    public async Task<IActionResult> GetById({{ pk_type }} id, CancellationToken ct)
    {
        var item = await _context.Set<{{ table.class_name }}>().FindAsync(new object?[] { id }, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] {{ table.class_name }} entity, CancellationToken ct)
    {
        _context.Set<{{ table.class_name }}>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return Ok(entity);
    }

    [HttpPut("{{'{'}}id{{'}'}}")]
    public async Task<IActionResult> Update({{ pk_type }} id, [FromBody] {{ table.class_name }} entity, CancellationToken ct)
    {
        _context.Set<{{ table.class_name }}>().Update(entity);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{{'{'}}id{{'}'}}")]
    public async Task<IActionResult> Delete({{ pk_type }} id, CancellationToken ct)
    {
        var item = await _context.Set<{{ table.class_name }}>().FindAsync(new object?[] { id }, ct);
        if (item is null) return NotFound();
        _context.Set<{{ table.class_name }}>().Remove(item);
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
"""
        env = Environment(loader=DictLoader({'t': template}))
        content = env.get_template('t').render(
            table=table,
            pk_type=pk_type,
        )
        
        out_file = self.output_dir / 'src/Pars.API/Controllers' / f'{table.class_name}sController.cs'
        out_file.write_text(content, encoding='utf-8')
    
    def _generate_dbcontext(self):
        # Collect all tables with columns
        valid_tables = [t for t in self.parser.tables.values() if t.columns]
        
        template = """// Auto-generated by Pars Scaffolding
using Microsoft.EntityFrameworkCore;
using Pars.Domain.Entities;

namespace Pars.Infrastructure.Persistence;

public class ParsDbContext : DbContext
{
    public ParsDbContext(DbContextOptions<ParsDbContext> options) : base(options) { }

{% for t in tables %}
    public DbSet<{{ t.class_name }}> {{ t.class_name }}s => Set<{{ t.class_name }}>();
{% endfor %}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
{% for fk in fks %}
        // {{ fk.source_table }}.{{ fk.source_column }} -> {{ fk.target_table }}.{{ fk.target_column }}
        modelBuilder.Entity<{{ fk.source_class }}>()
            .HasOne<{{ fk.target_class }}>()
            .WithMany()
            .HasForeignKey(e => e.{{ fk.source_prop }})
            .OnDelete(DeleteBehavior.Restrict);
{% endfor %}
    }
}
"""
        fks_data = []
        for fk in self.parser.foreign_keys:
            src_key = fk.source_table
            tgt_key = fk.target_table
            if src_key not in self.parser.tables or tgt_key not in self.parser.tables:
                continue
            if not self.parser.tables[src_key].columns or not self.parser.tables[tgt_key].columns:
                continue
            
            fks_data.append({
                'source_table': fk.source_table,
                'source_column': fk.source_column,
                'target_table': fk.target_table,
                'target_column': fk.target_column,
                'source_class': self.parser.tables[src_key].class_name,
                'target_class': self.parser.tables[tgt_key].class_name,
                'source_prop': safe_property_name(fk.source_column),
            })
        
        env = Environment(loader=DictLoader({'t': template}))
        content = env.get_template('t').render(
            tables=valid_tables,
            fks=fks_data,
        )
        content = re.sub(r'\n{4,}', '\n\n\n', content)
        
        out_file = self.output_dir / 'src/Pars.Infrastructure/Persistence/ParsDbContext.cs'
        out_file.write_text(content, encoding='utf-8')

# ============================================================
# 6. MAIN
# ============================================================

def main():
    html_file = "pars_documentation.html"
    output_dir = "ParsSystem"
    
    if not Path(html_file).exists():
        print(f"❌ File not found: {html_file}")
        print(f"Please save the HTML documentation to '{html_file}' first.")
        return
    
    print(f"📖 Reading {html_file}...")
    html_content = Path(html_file).read_text(encoding='utf-8')
    
    print("🔍 Parsing database structure...")
    parser = DatabaseParser(html_content)
    parser.parse()
    
    print(f"📊 Found {len(parser.tables)} tables")
    print(f"🔗 Found {len(parser.foreign_keys)} foreign keys")
    
    if Path(output_dir).exists():
        shutil.rmtree(output_dir)
    
    generator = CodeGenerator(parser, output_dir)
    generator.generate_all()

if __name__ == '__main__':
    main()