# NuBlox Mastered Package — MySQL

This directory is a NuBlox-mastered source snapshot of the `mysqljs/mysql` Node.js MySQL driver.

## Provenance

- Upstream repository: `https://github.com/mysqljs/mysql`
- Upstream version at initial mastering: `2.18.1`
- Initial upstream commit: `dc9c152a87ec51a1f647447268917243d2eab1fd`
- Initial mastering date: `2026-09-26`
- Upstream licence: MIT (retained as `License`)

## Mastering model

The upstream project is vendored as source rather than referenced as a Git submodule. NuBlox therefore owns and versions this copy independently inside the Enterprise Operating System repository while retaining upstream copyright and licence notices.

NuBlox-specific changes should be made in this mastered package and tracked through normal NuBlox pull requests. Upstream refreshes are explicit, reviewable sync operations so NuBlox changes cannot be silently overwritten by a moving upstream branch.

## Refresh

Run the `Sync MySQL mastered package` workflow with a reviewed, full 40-character upstream commit SHA. The workflow replaces the vendored upstream snapshot while recreating this NuBlox provenance marker.
