# Mongrow

It's a MongoDB migration helper.

With this, you can write classes that implement steps to migrate a MongoDB database.

Steps are identified by a number and a "branch specification". The branch specification allows for
co-existence of steps with the same number, thus escaping a global lock on the number sequence when
working with multiple branches.

The branch specification defaults to `master`. You are encouraged to structure your steps like this:

```
| 1 - master |                 |
| 2 - master |                 |
| 3 - master | 3 - some-branch |
|            | 4 - some-branch |
| 5 - master |                 |
```

and so forth.
