using BatteryMes.Api.Data;
using SqlSugar;

namespace BatteryMes.Api.Factory;

public sealed record FactoryNodeDto(
    string Nid,
    string Name,
    string? Description,
    string LevelNid,
    string LevelName,
    int Level,
    string? Pnid,
    IReadOnlyList<FactoryNodeDto> Children);

public sealed record SaveFactoryNodeRequest(string Name, string? Description, string? LevelNid, string? Pnid);

public sealed class FactoryModelService(ISqlSugarClient db)
{
    public IReadOnlyList<FactoryNodeDto> GetTree()
    {
        var levels = db.Queryable<FmLevel>().ToList().ToDictionary(item => item.Nid);
        var nodes = db.Queryable<FmNode>().Where(item => item.IsDeleted == 0).ToList();
        var byParent = nodes
            .GroupBy(item => item.Pnid ?? "")
            .ToDictionary(group => group.Key, group => group.OrderBy(item => item.SortNo).ThenBy(item => item.Name).ToList());

        FactoryNodeDto Map(FmNode node)
        {
            var level = levels[node.LevelNid];
            var children = byParent.TryGetValue(node.Nid, out var found)
                ? found.Select(Map).ToList()
                : [];
            return new FactoryNodeDto(node.Nid, node.Name, node.Description, node.LevelNid, level.Name, level.Level, node.Pnid, children);
        }

        return byParent.TryGetValue("", out var roots) ? roots.Select(Map).ToList() : [];
    }

    public FactoryNodeDto Create(SaveFactoryNodeRequest request)
    {
        var name = RequireName(request.Name);
        var levels = db.Queryable<FmLevel>().ToList();
        FmNode? parent = null;
        FmLevel level;
        if (string.IsNullOrWhiteSpace(request.Pnid))
        {
            level = levels.Single(item => item.Nid == "ENTERPRISE");
        }
        else
        {
            parent = db.Queryable<FmNode>()
                    .Where(item => item.Nid == request.Pnid && item.IsDeleted == 0)
                    .Take(1)
                    .ToList()
                    .FirstOrDefault()
                ?? throw new AppException(400, "上级节点不存在");
            var parentLevel = levels.Single(item => item.Nid == parent.LevelNid);
            level = levels.SingleOrDefault(item => item.Level == parentLevel.Level + 1)
                ?? throw new AppException(400, "工位下面不能再挂节点");
        }

        if (!string.IsNullOrWhiteSpace(request.LevelNid) && request.LevelNid != level.Nid)
        {
            throw new AppException(400, "层级与上级节点不匹配");
        }

        var now = DateTime.Now;
        var siblingQuery = db.Queryable<FmNode>().Where(item => item.IsDeleted == 0);
        siblingQuery = parent == null
            ? siblingQuery.Where(item => item.Pnid == null)
            : siblingQuery.Where(item => item.Pnid == parent.Nid);
        var siblings = siblingQuery.ToList();
        var node = new FmNode
        {
            Nid = Guid.NewGuid().ToString("N"),
            Name = name,
            Description = Clean(request.Description),
            LevelNid = level.Nid,
            Pnid = parent?.Nid,
            SortNo = siblings.Count == 0 ? 1 : siblings.Max(item => item.SortNo) + 1,
            IsDeleted = 0,
            CreateOn = now,
            LastUpdateOn = now,
        };
        db.Insertable(node).ExecuteCommand();
        return ToDto(node, level, []);
    }

    public FactoryNodeDto Update(string nid, SaveFactoryNodeRequest request)
    {
        var node = FindActive(nid);
        node.Name = RequireName(request.Name);
        node.Description = Clean(request.Description);
        node.LastUpdateOn = DateTime.Now;
        db.Updateable(node)
            .UpdateColumns(item => new { item.Name, item.Description, item.LastUpdateOn })
            .ExecuteCommand();
        var level = db.Queryable<FmLevel>().Where(item => item.Nid == node.LevelNid).Take(1).ToList().Single();
        return ToDto(node, level, []);
    }

    public void Delete(string nid)
    {
        FindActive(nid);
        if (db.Queryable<FmNode>().Any(item => item.Pnid == nid && item.IsDeleted == 0))
        {
            throw new AppException(400, "请先删除下级节点");
        }

        db.Updateable<FmNode>()
            .SetColumns(item => item.IsDeleted == 1)
            .SetColumns(item => item.LastUpdateOn == DateTime.Now)
            .Where(item => item.Nid == nid)
            .ExecuteCommand();
    }

    private FmNode FindActive(string nid)
    {
        return db.Queryable<FmNode>()
                .Where(item => item.Nid == nid && item.IsDeleted == 0)
                .Take(1)
                .ToList()
                .FirstOrDefault()
            ?? throw new AppException(404, "节点不存在");
    }

    private static string RequireName(string? name)
    {
        var cleaned = name?.Trim() ?? "";
        if (cleaned.Length == 0 || cleaned.Length > 200)
        {
            throw new AppException(400, "名称不能为空，且不超过 200 个字");
        }

        return cleaned;
    }

    private static string? Clean(string? value)
    {
        var cleaned = value?.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            return null;
        }

        if (cleaned.Length > 500)
        {
            throw new AppException(400, "说明不超过 500 个字");
        }

        return cleaned;
    }

    private static FactoryNodeDto ToDto(FmNode node, FmLevel level, IReadOnlyList<FactoryNodeDto> children)
    {
        return new FactoryNodeDto(node.Nid, node.Name, node.Description, node.LevelNid, level.Name, level.Level, node.Pnid, children);
    }
}
