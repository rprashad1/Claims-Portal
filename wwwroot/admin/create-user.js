async function loadLookups() {
  const [rolesRes, groupsRes] = await Promise.all([
    fetch('/api/admin/roles'),
    fetch('/api/admin/groups')
  ]);
  const roles = await rolesRes.json();
  const groups = await groupsRes.json();

  const roleSel = document.getElementById('role');
  roles.forEach(r => { const o = document.createElement('option'); o.value = r.roleId; o.text = r.roleName; roleSel.appendChild(o); });

  const groupSel = document.getElementById('group');
  groups.forEach(g => { const o = document.createElement('option'); o.value = g.groupId; o.text = g.groupName; groupSel.appendChild(o); });
}

document.getElementById('createForm').addEventListener('submit', async (e) => {
  e.preventDefault();
  const dto = {
    username: document.getElementById('username').value,
    password: document.getElementById('password').value,
    fullName: document.getElementById('fullname').value,
    email: document.getElementById('email').value,
    telephone: document.getElementById('telephone').value,
    extension: document.getElementById('extension').value,
    status: document.getElementById('status').value,
    startDate: document.getElementById('startdate').value || new Date().toISOString(),
    endDate: document.getElementById('enddate').value || null,
    assignmentGroupId: parseInt(document.getElementById('group').value) || null,
    roleId: parseInt(document.getElementById('role').value) || null,
    supervisorUserId: document.getElementById('supervisor').value ? parseInt(document.getElementById('supervisor').value) : null,
    expenseReserve: document.getElementById('expenseReserve').value || null,
    indemnityReserve: document.getElementById('indemnityReserve').value || null,
    expensePayment: document.getElementById('expensePayment').value || null,
    indemnityPayment: document.getElementById('indemnityPayment').value || null,
    createdBy: 'admin-ui'
  };

  const res = await fetch('/api/admin/users', {
    method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(dto)
  });
  const resultSpan = document.getElementById('result');
  if (res.status === 201) {
    const data = await res.json();
    resultSpan.textContent = 'Created user ' + (data.username || data.userId);
  } else {
    const text = await res.text();
    resultSpan.textContent = 'Error: ' + res.status + ' ' + text;
  }
});

loadLookups().catch(err => console.error(err));
